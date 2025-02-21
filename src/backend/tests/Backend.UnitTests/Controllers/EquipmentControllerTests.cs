using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.API.Controllers;
using Backend.Core.DTOs.Equipment;
using Backend.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Backend.UnitTests.Controllers
{
    [TestClass]
    public class EquipmentControllerTests
    {
        private Mock<IEquipmentRepository> _mockRepository;
        private Mock<ILogger<EquipmentController>> _mockLogger;
        private EquipmentController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockRepository = new Mock<IEquipmentRepository>();
            _mockLogger = new Mock<ILogger<EquipmentController>>();
            _controller = new EquipmentController(_mockRepository.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetByCompanyAsync_ValidCompanyId_ReturnsEquipmentList()
        {
            // Arrange
            int companyId = 1;
            var expectedEquipment = new List<EquipmentDto>
            {
                new EquipmentDto
                {
                    Id = 1,
                    Model = "TestModel",
                    SerialNumber = "SN123",
                    Description = "Test Equipment",
                    Condition = "Good",
                    IsOut = false,
                    CompanyId = companyId
                }
            };

            _mockRepository.Setup(r => r.GetByCompanyAsync(
                    It.IsAny<int>(),
                    It.IsAny<EquipmentFilter>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEquipment);

            // Act
            var result = await _controller.GetByCompanyAsync(companyId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            var returnedEquipment = okResult.Value as IEnumerable<EquipmentDto>;
            CollectionAssert.AreEqual(expectedEquipment, new List<EquipmentDto>(returnedEquipment));
            _mockRepository.Verify(r => r.GetByCompanyAsync(
                companyId,
                It.IsAny<EquipmentFilter>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetByCompanyAsync_InvalidCompanyId_ReturnsBadRequest()
        {
            // Arrange
            int invalidCompanyId = 0;

            // Act
            var result = await _controller.GetByCompanyAsync(invalidCompanyId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockRepository.Verify(r => r.GetByCompanyAsync(
                It.IsAny<int>(),
                It.IsAny<EquipmentFilter>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AssignToInspectorAsync_ValidAssignment_ReturnsUpdatedEquipment()
        {
            // Arrange
            var assignmentDto = new EquipmentAssignmentDto
            {
                EquipmentId = 1,
                InspectorId = 1,
                Condition = "Good",
                AssignmentDate = DateTime.UtcNow
            };

            var expectedEquipment = new EquipmentDto
            {
                Id = assignmentDto.EquipmentId,
                IsOut = true,
                AssignedToInspectorId = assignmentDto.InspectorId,
                AssignedCondition = assignmentDto.Condition,
                AssignedDate = assignmentDto.AssignmentDate
            };

            _mockRepository.Setup(r => r.AssignToInspectorAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEquipment);

            // Act
            var result = await _controller.AssignToInspectorAsync(assignmentDto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(expectedEquipment, okResult.Value);
            _mockRepository.Verify(r => r.AssignToInspectorAsync(
                assignmentDto.EquipmentId,
                assignmentDto.InspectorId,
                assignmentDto.Condition,
                assignmentDto.AssignmentDate,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AssignToInspectorAsync_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var invalidAssignmentDto = new EquipmentAssignmentDto(); // Missing required fields
            _controller.ModelState.AddModelError("EquipmentId", "Required");

            // Act
            var result = await _controller.AssignToInspectorAsync(invalidAssignmentDto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockRepository.Verify(r => r.AssignToInspectorAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task RecordReturnAsync_ValidReturn_ReturnsUpdatedEquipment()
        {
            // Arrange
            var returnDto = new EquipmentReturnDto
            {
                EquipmentId = 1,
                ReturnCondition = "Good",
                ReturnDate = DateTime.UtcNow
            };

            var expectedEquipment = new EquipmentDto
            {
                Id = returnDto.EquipmentId,
                IsOut = false,
                ReturnedCondition = returnDto.ReturnCondition,
                ReturnedDate = returnDto.ReturnDate
            };

            _mockRepository.Setup(r => r.RecordReturnAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEquipment);

            // Act
            var result = await _controller.RecordReturnAsync(returnDto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(expectedEquipment, okResult.Value);
            _mockRepository.Verify(r => r.RecordReturnAsync(
                returnDto.EquipmentId,
                returnDto.ReturnCondition,
                returnDto.ReturnDate,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task RecordReturnAsync_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var invalidReturnDto = new EquipmentReturnDto(); // Missing required fields
            _controller.ModelState.AddModelError("EquipmentId", "Required");

            // Act
            var result = await _controller.RecordReturnAsync(invalidReturnDto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            _mockRepository.Verify(r => r.RecordReturnAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task GetByCompanyAsync_RepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            int companyId = 1;
            _mockRepository.Setup(r => r.GetByCompanyAsync(
                    It.IsAny<int>(),
                    It.IsAny<EquipmentFilter>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetByCompanyAsync(companyId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var statusResult = result.Result as ObjectResult;
            Assert.AreEqual(500, statusResult.StatusCode);
        }

        [TestMethod]
        public async Task AssignToInspectorAsync_RepositoryThrowsUnauthorized_ReturnsForbid()
        {
            // Arrange
            var assignmentDto = new EquipmentAssignmentDto
            {
                EquipmentId = 1,
                InspectorId = 1,
                Condition = "Good",
                AssignmentDate = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.AssignToInspectorAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.AssignToInspectorAsync(assignmentDto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
        }
    }
}