using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Backend.Core.DTOs.Equipment;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Backend.IntegrationTests.Controllers
{
    /// <summary>
    /// Integration tests for Equipment Controller endpoints covering CRUD operations,
    /// permissions, validations, and error scenarios
    /// </summary>
    [Collection("Database")]
    public class EquipmentControllerTests : IDisposable
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public EquipmentControllerTests(ITestOutputHelper output)
        {
            _output = output;
            _factory = new CustomWebApplicationFactory<Startup>();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetByCompanyAsync_WithValidPermissions_ReturnsEquipmentList()
        {
            // Arrange
            const int companyId = 1;

            // Act
            var response = await _client.GetAsync($"/api/equipment/company/{companyId}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var equipment = await response.Content.ReadFromJsonAsync<EquipmentDto[]>();
            equipment.Should().NotBeNull();
            equipment.Should().AllSatisfy(e =>
            {
                e.Model.Should().NotBeNullOrEmpty();
                e.SerialNumber.Should().NotBeNullOrEmpty();
                e.Condition.Should().NotBeNullOrEmpty();
                e.CompanyId.Should().Be(companyId);
            });
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetByCompanyAsync_WithoutPermissions_ReturnsForbidden()
        {
            // Arrange
            const int companyId = 1;
            _client.DefaultRequestHeaders.Remove("Authorization");

            // Act
            var response = await _client.GetAsync($"/api/equipment/company/{companyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task AssignToInspectorAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var assignmentDto = new EquipmentAssignmentDto
            {
                EquipmentId = 1,
                InspectorId = 1,
                Condition = "Good",
                AssignmentDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/equipment/assign", assignmentDto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var equipment = await response.Content.ReadFromJsonAsync<EquipmentDto>();
            equipment.Should().NotBeNull();
            equipment.IsOut.Should().BeTrue();
            equipment.AssignedToInspectorId.Should().Be(assignmentDto.InspectorId);
            equipment.AssignedCondition.Should().Be(assignmentDto.Condition);
            equipment.AssignedDate.Should().BeCloseTo(assignmentDto.AssignmentDate, TimeSpan.FromSeconds(1));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task AssignToInspectorAsync_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var assignmentDto = new EquipmentAssignmentDto
            {
                EquipmentId = 0,
                InspectorId = 0,
                Condition = "",
                AssignmentDate = DateTime.UtcNow.AddDays(1) // Future date
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/equipment/assign", assignmentDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RecordReturnAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var returnDto = new EquipmentReturnDto
            {
                EquipmentId = 1,
                ReturnCondition = "Used - Good",
                ReturnDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/equipment/return", returnDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var equipment = await response.Content.ReadFromJsonAsync<EquipmentDto>();
            equipment.Should().NotBeNull();
            equipment.IsOut.Should().BeFalse();
            equipment.ReturnedCondition.Should().Be(returnDto.ReturnCondition);
            equipment.ReturnedDate.Should().BeCloseTo(returnDto.ReturnDate, TimeSpan.FromSeconds(1));
            equipment.AssignedToInspectorId.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RecordReturnAsync_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var returnDto = new EquipmentReturnDto
            {
                EquipmentId = 0,
                ReturnCondition = "",
                ReturnDate = DateTime.UtcNow.AddDays(1) // Future date
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/equipment/return", returnDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task AssignToInspectorAsync_WithAlreadyAssignedEquipment_ReturnsConflict()
        {
            // Arrange
            var assignmentDto = new EquipmentAssignmentDto
            {
                EquipmentId = 2, // Already assigned equipment
                InspectorId = 1,
                Condition = "Good",
                AssignmentDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/equipment/assign", assignmentDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RecordReturnAsync_WithUnassignedEquipment_ReturnsBadRequest()
        {
            // Arrange
            var returnDto = new EquipmentReturnDto
            {
                EquipmentId = 3, // Unassigned equipment
                ReturnCondition = "Good",
                ReturnDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/equipment/return", returnDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        public void Dispose()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }
    }
}