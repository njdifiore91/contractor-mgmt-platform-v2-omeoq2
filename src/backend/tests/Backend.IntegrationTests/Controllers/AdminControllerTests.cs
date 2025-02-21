using Backend.Core.DTOs.Admin;
using Backend.Core.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
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
    [Collection("IntegrationTests")]
    public class AdminControllerTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public AdminControllerTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true,
                MaxAutomaticRedirections = 7
            });
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public async Task GetUsers_WithValidPermissions_ReturnsUsersList()
        {
            // Arrange
            var expectedUsers = new List<UserDto>
            {
                new UserDto
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@test.com",
                    EmailConfirmed = true,
                    Roles = new List<string> { "Admin" }
                }
            };

            // Act
            var response = await _client.GetAsync("/api/admin/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            users.Should().NotBeNull();
            users.Should().HaveCountGreaterThan(0);
            users[0].Email.Should().Be(expectedUsers[0].Email);
            response.Headers.Should().ContainKey("X-Pagination");
        }

        [Fact]
        public async Task CreateUser_WithValidData_ReturnsCreatedUser()
        {
            // Arrange
            var newUser = new UserDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test.user@example.com",
                Roles = new List<string> { "User" },
                ReceiveEmails = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/admin/users", newUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
            createdUser.Should().NotBeNull();
            createdUser.FirstName.Should().Be(newUser.FirstName);
            createdUser.LastName.Should().Be(newUser.LastName);
            createdUser.Email.Should().Be(newUser.Email);
        }

        [Fact]
        public async Task UpdateUser_WithValidData_ReturnsUpdatedUser()
        {
            // Arrange
            var userId = 1; // Using seeded test user
            var updateData = new UserDto
            {
                Id = userId,
                FirstName = "Updated",
                LastName = "User",
                Email = "updated.user@example.com",
                Roles = new List<string> { "User" },
                ReceiveEmails = true
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/admin/users/{userId}", updateData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedUser = await response.Content.ReadFromJsonAsync<UserDto>();
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be(updateData.FirstName);
            updatedUser.LastName.Should().Be(updateData.LastName);
            updatedUser.Email.Should().Be(updateData.Email);
        }

        [Fact]
        public async Task DeleteUser_ExistingUser_ReturnsNoContent()
        {
            // Arrange
            var newUser = new UserDto
            {
                FirstName = "ToDelete",
                LastName = "User",
                Email = "todelete@example.com",
                Roles = new List<string> { "User" }
            };
            var createResponse = await _client.PostAsJsonAsync("/api/admin/users", newUser);
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/admin/users/{createdUser.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var getResponse = await _client.GetAsync($"/api/admin/users/{createdUser.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetQuickLinks_WithValidPermissions_ReturnsQuickLinksList()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/admin/quicklinks");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var quickLinks = await response.Content.ReadFromJsonAsync<List<QuickLink>>();
            quickLinks.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateQuickLink_WithValidData_ReturnsCreatedQuickLink()
        {
            // Arrange
            var newQuickLink = new QuickLink
            {
                Label = "Test Link",
                Link = "https://test.com",
                Order = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/admin/quicklinks", newQuickLink);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdLink = await response.Content.ReadFromJsonAsync<QuickLink>();
            createdLink.Should().NotBeNull();
            createdLink.Label.Should().Be(newQuickLink.Label);
            createdLink.Link.Should().Be(newQuickLink.Link);
        }

        [Fact]
        public async Task GetCodeTypes_WithValidPermissions_ReturnsCodeTypesList()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/admin/codetypes");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var codeTypes = await response.Content.ReadFromJsonAsync<List<CodeType>>();
            codeTypes.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateCodeType_WithValidData_ReturnsCreatedCodeType()
        {
            // Arrange
            var newCodeType = new CodeType
            {
                Name = "Test Code Type",
                Description = "Test Description",
                IsExpirable = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/admin/codetypes", newCodeType);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdCodeType = await response.Content.ReadFromJsonAsync<CodeType>();
            createdCodeType.Should().NotBeNull();
            createdCodeType.Name.Should().Be(newCodeType.Name);
        }

        public void Dispose()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }
    }

    public class CodeType
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsExpirable { get; set; }
        public List<Code> Codes { get; set; } = new List<Code>();
    }

    public class Code
    {
        public string Value { get; set; }
        public string Description { get; set; }
        public bool IsExpirable { get; set; }
    }
}