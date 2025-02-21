using Backend.API;
using Backend.Core.Entities;
using Backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.Test;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Backend.IntegrationTests.Controllers
{
    [Collection("Database")]
    public class InspectorControllerTests : IDisposable
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;
        private readonly Mock<IEmailService> _emailServiceMock;

        public InspectorControllerTests(ITestOutputHelper output)
        {
            _output = output;
            _factory = new CustomWebApplicationFactory();
            _emailServiceMock = new Mock<IEmailService>();

            // Configure test services
            _factory.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => _emailServiceMock.Object);
            });

            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task SearchInspectors_WithValidZipAndRadius_ReturnsMatchingInspectors()
        {
            // Arrange
            var zipCode = "77001";
            var radius = 50;
            var url = $"/api/v1/inspectors/search?zipCode={zipCode}&radius={radius}";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var inspectors = await response.Content.ReadFromJsonAsync<List<Inspector>>();
            inspectors.Should().NotBeNull();
            inspectors.Should().AllSatisfy(inspector =>
            {
                inspector.State.Should().NotBeNullOrEmpty();
                inspector.InspectorId.Should().NotBeNullOrEmpty();
                inspector.Status.Should().NotBeNullOrEmpty();
            });
        }

        [Fact]
        public async Task MobilizeInspector_WithValidData_ReturnsMobilizationDetails()
        {
            // Arrange
            var inspectorId = 1;
            var mobilizationRequest = new
            {
                MobilizationDate = DateTime.UtcNow.AddDays(1),
                HireType = "Full-Time",
                ProjectLocation = "Houston, TX",
                Classification = "Level 1",
                CustomerId = 1,
                ContractId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/v1/inspectors/{inspectorId}/mobilize", mobilizationRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _emailServiceMock.Verify(x => x.SendMobilizationEmail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<DateTime>(d => d == mobilizationRequest.MobilizationDate)),
                Times.Once);
        }

        [Fact]
        public async Task DemobilizeInspector_WithValidData_ReturnsDemobilizationDetails()
        {
            // Arrange
            var inspectorId = 1;
            var demobilizationRequest = new
            {
                DemobilizationDate = DateTime.UtcNow,
                Reason = "Project Completed",
                Note = "Excellent performance"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/v1/inspectors/{inspectorId}/demobilize", demobilizationRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Inspector>();
            result.Should().NotBeNull();
            result.DemobilizationDate.Should().Be(demobilizationRequest.DemobilizationDate);
            result.DemobilizationReason.Should().Be(demobilizationRequest.Reason);
        }

        [Fact]
        public async Task CreateDrugTest_WithValidData_ReturnsDrugTestDetails()
        {
            // Arrange
            var inspectorId = 1;
            var drugTestRequest = new
            {
                TestDate = DateTime.UtcNow,
                TestType = "Random",
                Frequency = "Quarterly",
                Result = "Negative",
                Comment = "Standard test",
                Company = "TestLab Inc"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/v1/inspectors/{inspectorId}/drugtests", drugTestRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<DrugTest>();
            result.Should().NotBeNull();
            result.TestDate.Should().Be(drugTestRequest.TestDate);
            result.TestType.Should().Be(drugTestRequest.TestType);
            result.Result.Should().Be(drugTestRequest.Result);
        }

        [Fact]
        public async Task AssignEquipment_WithValidData_ReturnsAssignmentDetails()
        {
            // Arrange
            var inspectorId = 1;
            var equipmentId = 1;
            var assignmentRequest = new
            {
                AssignmentDate = DateTime.UtcNow,
                Condition = "Excellent",
                Notes = "New assignment"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                $"/api/v1/inspectors/{inspectorId}/equipment/{equipmentId}/assign",
                assignmentRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Equipment>();
            result.Should().NotBeNull();
            result.AssignedToInspectorId.Should().Be(inspectorId);
            result.AssignedDate.Should().Be(assignmentRequest.AssignmentDate);
            result.Condition.Should().Be(assignmentRequest.Condition);
        }

        [Fact]
        public async Task ReturnEquipment_WithValidData_ReturnsUpdatedEquipment()
        {
            // Arrange
            var inspectorId = 1;
            var equipmentId = 1;
            var returnRequest = new
            {
                ReturnDate = DateTime.UtcNow,
                ReturnedCondition = "Good",
                Notes = "Normal wear and tear"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                $"/api/v1/inspectors/{inspectorId}/equipment/{equipmentId}/return",
                returnRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Equipment>();
            result.Should().NotBeNull();
            result.IsOut.Should().BeFalse();
            result.ReturnedDate.Should().Be(returnRequest.ReturnDate);
            result.ReturnedCondition.Should().Be(returnRequest.ReturnedCondition);
        }

        public void Dispose()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }
    }
}