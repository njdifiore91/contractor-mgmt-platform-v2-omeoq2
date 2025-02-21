using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.API.Controllers;
using Backend.Core.Entities;
using Backend.Core.Interfaces.Repositories;
using Backend.Core.Interfaces.Services;
using FluentAssertions;                // v6.2.0
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;  // v2.2.0
using Moq;                             // v4.16.1

namespace Backend.UnitTests.Controllers
{
    [TestClass]
    public class InspectorControllerTests
    {
        private Mock<IInspectorRepository> _mockRepository;
        private Mock<IEmailService> _mockEmailService;
        private InspectorController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockRepository = new Mock<IInspectorRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _controller = new InspectorController(_mockRepository.Object, _mockEmailService.Object, null, null);
        }

        [TestMethod]
        public async Task SearchAsync_ValidParameters_ReturnsInspectors()
        {
            // Arrange
            var zipCode = "12345";
            var radiusMiles = 50;
            var filters = new Dictionary<string, string>
            {
                { "status", "active" },
                { "specialty", "welding" }
            };

            var expectedResult = new SearchResult<Inspector>
            {
                Items = new List<Inspector>
                {
                    new Inspector { Id = 1, FirstName = "John", LastName = "Doe" },
                    new Inspector { Id = 2, FirstName = "Jane", LastName = "Smith" }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.SearchInspectorsAsync(zipCode, radiusMiles, It.IsAny<SearchFilters>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.SearchAsync(zipCode, radiusMiles, filters);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var searchResult = okResult.Value.Should().BeOfType<SearchResult<Inspector>>().Subject;
            searchResult.TotalCount.Should().Be(2);
            searchResult.Items.Should().HaveCount(2);
            _mockRepository.Verify(r => r.SearchInspectorsAsync(zipCode, radiusMiles, It.IsAny<SearchFilters>()), Times.Once);
        }

        [TestMethod]
        public async Task MobilizeAsync_ValidInspector_SendsEmailAndReturnsSuccess()
        {
            // Arrange
            var inspectorId = 1;
            var mobilizationDetails = new MobilizationDetails
            {
                ProjectName = "Test Project",
                CustomerName = "Test Customer",
                MobilizationDate = DateTime.Now.AddDays(7),
                HireType = "Full-Time",
                Classification = "Level 1",
                Department = "QA",
                Location = "Site A"
            };

            var inspector = new Inspector
            {
                Id = inspectorId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(inspectorId))
                .ReturnsAsync(inspector);
            _mockRepository.Setup(r => r.MobilizeInspectorAsync(inspectorId, mobilizationDetails))
                .ReturnsAsync(new MobilizationResult { Success = true });
            _mockEmailService.Setup(e => e.SendMobilizationNotificationAsync(
                inspector.Email,
                mobilizationDetails.ProjectName,
                mobilizationDetails.CustomerName,
                mobilizationDetails.MobilizationDate))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.MobilizeAsync(inspectorId, mobilizationDetails);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockRepository.Verify(r => r.MobilizeInspectorAsync(inspectorId, mobilizationDetails), Times.Once);
            _mockEmailService.Verify(e => e.SendMobilizationNotificationAsync(
                inspector.Email,
                mobilizationDetails.ProjectName,
                mobilizationDetails.CustomerName,
                mobilizationDetails.MobilizationDate), Times.Once);
        }

        [TestMethod]
        public async Task DemobilizeAsync_ValidInspector_ReturnsSuccess()
        {
            // Arrange
            var inspectorId = 1;
            var demobilizationDetails = new DemobilizationDetails
            {
                DemobilizationDate = DateTime.Now,
                Reason = "Project Complete",
                Notes = "Excellent performance"
            };

            var inspector = new Inspector { Id = inspectorId };

            _mockRepository.Setup(r => r.GetByIdAsync(inspectorId))
                .ReturnsAsync(inspector);
            _mockRepository.Setup(r => r.DemobilizeInspectorAsync(inspectorId, demobilizationDetails))
                .ReturnsAsync(new DemobilizationResult { Success = true });

            // Act
            var result = await _controller.DemobilizeAsync(inspectorId, demobilizationDetails);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockRepository.Verify(r => r.DemobilizeInspectorAsync(inspectorId, demobilizationDetails), Times.Once);
        }

        [TestMethod]
        public async Task AddDrugTestAsync_ValidTest_ReturnsSuccess()
        {
            // Arrange
            var inspectorId = 1;
            var drugTestRecord = new DrugTestRecord
            {
                TestDate = DateTime.Now,
                TestType = "Random",
                Frequency = "Quarterly",
                Result = "Negative",
                Comment = "Standard test"
            };

            var inspector = new Inspector { Id = inspectorId };

            _mockRepository.Setup(r => r.GetByIdAsync(inspectorId))
                .ReturnsAsync(inspector);
            _mockRepository.Setup(r => r.ManageDrugTestAsync(inspectorId, drugTestRecord))
                .ReturnsAsync(new DrugTestResult { Success = true });

            // Act
            var result = await _controller.AddDrugTestAsync(inspectorId, drugTestRecord);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockRepository.Verify(r => r.ManageDrugTestAsync(inspectorId, drugTestRecord), Times.Once);
        }

        [TestMethod]
        public async Task AssignEquipmentAsync_ValidAssignment_ReturnsSuccess()
        {
            // Arrange
            var inspectorId = 1;
            var equipmentAssignment = new EquipmentAssignment
            {
                EquipmentId = 100,
                OutDate = DateTime.Now,
                OutCondition = "Excellent",
                Description = "Safety Equipment"
            };

            var inspector = new Inspector { Id = inspectorId };

            _mockRepository.Setup(r => r.GetByIdAsync(inspectorId))
                .ReturnsAsync(inspector);
            _mockRepository.Setup(r => r.ManageEquipmentAssignmentAsync(inspectorId, equipmentAssignment))
                .ReturnsAsync(new EquipmentAssignmentResult { Success = true });

            // Act
            var result = await _controller.AssignEquipmentAsync(inspectorId, equipmentAssignment);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockRepository.Verify(r => r.ManageEquipmentAssignmentAsync(inspectorId, equipmentAssignment), Times.Once);
        }
    }
}