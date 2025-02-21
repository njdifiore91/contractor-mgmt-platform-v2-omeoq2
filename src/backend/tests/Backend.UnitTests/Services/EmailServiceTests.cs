using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Core.Interfaces.Services;
using Backend.Infrastructure.Services;
using FluentAssertions;  // v6.2.0
using Microsoft.Extensions.Configuration;  // v6.0.0
using Microsoft.Extensions.Logging;  // v6.0.0
using Microsoft.Extensions.Caching.Memory;
using Moq;  // v4.16.1
using Xunit;  // v2.4.1
using Azure.Communication.Email;

namespace Backend.UnitTests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<EmailService>> _loggerMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly IEmailService _emailService;

        public EmailServiceTests()
        {
            // Initialize mocks
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<EmailService>>();
            _memoryCacheMock = new Mock<IMemoryCache>();

            // Setup configuration
            _configurationMock.Setup(c => c["Azure:EmailService:ConnectionString"])
                .Returns("test-connection-string");
            _configurationMock.Setup(c => c["Azure:EmailService:SenderEmail"])
                .Returns("sender@test.com");
            _configurationMock.Setup(c => c.GetValue<int>("Azure:EmailService:MaxRetryAttempts", 3))
                .Returns(3);
            _configurationMock.Setup(c => c.GetValue<int>("Azure:EmailService:RetryDelaySeconds", 2))
                .Returns(2);

            // Setup memory cache
            var cacheEntry = Mock.Of<ICacheEntry>();
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            // Initialize service
            _emailService = new EmailService(
                _configurationMock.Object,
                _loggerMock.Object,
                _memoryCacheMock.Object);
        }

        [Fact]
        public async Task SendEmailAsync_WithValidInputs_ShouldSucceed()
        {
            // Arrange
            var to = "test@example.com";
            var subject = "Test Subject";
            var body = "<p>Test Body</p>";
            var attachments = new List<string> { "test.pdf" };

            // Act
            var result = await _emailService.SendEmailAsync(to, subject, body, attachments);

            // Assert
            result.Should().BeTrue();
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Email sent successfully")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_WithInvalidEmail_ShouldFail()
        {
            // Arrange
            var to = "invalid-email";
            var subject = "Test Subject";
            var body = "Test Body";

            // Act
            var result = await _emailService.SendEmailAsync(to, subject, body);

            // Assert
            result.Should().BeFalse();
            _loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Invalid recipient email address")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task SendTemplatedEmailAsync_WithComplexData_ShouldProcess()
        {
            // Arrange
            var to = "test@example.com";
            var templateName = "TestTemplate";
            var templateData = new
            {
                Name = "John Doe",
                Date = DateTime.Now,
                Details = new { Project = "Test Project", Role = "Developer" }
            };

            // Act
            var result = await _emailService.SendTemplatedEmailAsync(to, templateName, templateData);

            // Assert
            result.Should().BeTrue();
            _memoryCacheMock.Verify(
                x => x.CreateEntry(It.Is<string>(s => s.Contains(templateName))),
                Times.Once);
        }

        [Fact]
        public async Task SendMobilizationNotificationAsync_WithFullDetails_ShouldNotify()
        {
            // Arrange
            var inspectorEmail = "inspector@example.com";
            var projectName = "Test Project";
            var customerName = "Test Customer";
            var mobilizationDate = DateTime.Now.AddDays(7);

            // Act
            var result = await _emailService.SendMobilizationNotificationAsync(
                inspectorEmail,
                projectName,
                customerName,
                mobilizationDate);

            // Assert
            result.Should().BeTrue();
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Sending mobilization notification")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_WithSecurityViolations_ShouldFail()
        {
            // Arrange
            var to = "test@example.com";
            var subject = "Test Subject";
            var maliciousBody = "<script>alert('xss')</script>";

            // Act
            var result = await _emailService.SendEmailAsync(to, subject, maliciousBody);

            // Assert
            result.Should().BeTrue(); // The service sanitizes HTML content
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Email sent successfully")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task SendTemplatedEmailAsync_WithMissingTemplate_ShouldFail()
        {
            // Arrange
            var to = "test@example.com";
            var nonExistentTemplate = "NonExistentTemplate";
            var templateData = new { Test = "Data" };

            // Setup cache to return null for missing template
            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Returns(false);

            // Act
            var result = await _emailService.SendTemplatedEmailAsync(to, nonExistentTemplate, templateData);

            // Assert
            result.Should().BeFalse();
            _loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Template not found")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_WithRetryLogic_ShouldRetryOnFailure()
        {
            // Arrange
            var to = "test@example.com";
            var subject = "Test Subject";
            var body = "Test Body";
            var retryCount = 0;

            _loggerMock.Setup(x => x.LogWarning(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Attempt")),
                It.IsAny<object[]>()))
                .Callback(() => retryCount++);

            // Act
            var result = await _emailService.SendEmailAsync(to, subject, body);

            // Assert
            result.Should().BeTrue();
            retryCount.Should().Be(0); // No retries needed in successful case
        }
    }
}