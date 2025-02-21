using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.API.Controllers;
using Backend.Core.DTOs.Customer;
using Backend.Core.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.UnitTests.Controllers
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly Mock<ILogger<CustomerController>> _mockLogger;
        private readonly CustomerController _controller;
        private readonly List<CustomerDto> _testCustomers;

        public CustomerControllerTests()
        {
            // Initialize mocks
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockLogger = new Mock<ILogger<CustomerController>>();
            _controller = new CustomerController(_mockCustomerRepository.Object, _mockLogger.Object);

            // Setup test data
            _testCustomers = new List<CustomerDto>
            {
                new CustomerDto
                {
                    Id = 1,
                    Name = "Test Company 1",
                    Code = "TC1",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "TestUser",
                    Contacts = new List<ContactDto>(),
                    Contracts = new List<ContractDto>()
                },
                new CustomerDto
                {
                    Id = 2,
                    Name = "Test Company 2",
                    Code = "TC2",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "TestUser",
                    Contacts = new List<ContactDto>(),
                    Contracts = new List<ContractDto>()
                }
            };
        }

        [Fact]
        public async Task GetAllAsync_WhenCustomersExist_ReturnsOkResultWithCustomers()
        {
            // Arrange
            _mockCustomerRepository.Setup(repo => repo.GetAllAsync(
                It.IsAny<bool>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testCustomers);

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var customers = okResult.Value.Should().BeAssignableTo<IEnumerable<CustomerDto>>().Subject;
            customers.Should().HaveCount(2);
            customers.Should().BeEquivalentTo(_testCustomers);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoCustomers_ReturnsOkResultWithEmptyList()
        {
            // Arrange
            _mockCustomerRepository.Setup(repo => repo.GetAllAsync(
                It.IsAny<bool>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CustomerDto>());

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var customers = okResult.Value.Should().BeAssignableTo<IEnumerable<CustomerDto>>().Subject;
            customers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_WhenCustomerExists_ReturnsOkResultWithCustomer()
        {
            // Arrange
            var expectedCustomer = _testCustomers[0];
            _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(
                expectedCustomer.Id, 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCustomer);

            // Act
            var result = await _controller.GetByIdAsync(expectedCustomer.Id);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var customer = okResult.Value.Should().BeOfType<CustomerDto>().Subject;
            customer.Should().BeEquivalentTo(expectedCustomer);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCustomerDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(
                It.IsAny<int>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((CustomerDto)null);

            // Act
            var result = await _controller.GetByIdAsync(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task SearchAsync_WithValidTerm_ReturnsOkResultWithMatchingCustomers()
        {
            // Arrange
            var searchTerm = "Test";
            _mockCustomerRepository.Setup(repo => repo.SearchAsync(
                searchTerm,
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testCustomers);

            // Act
            var result = await _controller.SearchAsync(searchTerm);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var customers = okResult.Value.Should().BeAssignableTo<IEnumerable<CustomerDto>>().Subject;
            customers.Should().BeEquivalentTo(_testCustomers);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task SearchAsync_WithInvalidSearchTerm_ReturnsBadRequest(string searchTerm)
        {
            // Act
            var result = await _controller.SearchAsync(searchTerm);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_WithValidCustomer_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var newCustomer = new CustomerDto
            {
                Name = "New Test Company",
                Code = "NTC",
                IsActive = true,
                CreatedBy = "TestUser"
            };

            var createdCustomer = new CustomerDto
            {
                Id = 3,
                Name = newCustomer.Name,
                Code = newCustomer.Code,
                IsActive = newCustomer.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = newCustomer.CreatedBy
            };

            _mockCustomerRepository.Setup(repo => repo.AddAsync(
                It.IsAny<CustomerDto>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdCustomer);

            // Act
            var result = await _controller.CreateAsync(newCustomer);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(CustomerController.GetByIdAsync));
            createdAtActionResult.RouteValues["id"].Should().Be(createdCustomer.Id);
            var returnedCustomer = createdAtActionResult.Value.Should().BeOfType<CustomerDto>().Subject;
            returnedCustomer.Should().BeEquivalentTo(createdCustomer);
        }

        [Fact]
        public async Task CreateAsync_WithNullCustomer_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateAsync(null);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_WithValidCustomer_ReturnsOkResultWithUpdatedCustomer()
        {
            // Arrange
            var customerToUpdate = _testCustomers[0];
            customerToUpdate.Name = "Updated Test Company";

            _mockCustomerRepository.Setup(repo => repo.UpdateAsync(
                It.IsAny<CustomerDto>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(customerToUpdate);

            // Act
            var result = await _controller.UpdateAsync(customerToUpdate.Id, customerToUpdate);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var updatedCustomer = okResult.Value.Should().BeOfType<CustomerDto>().Subject;
            updatedCustomer.Should().BeEquivalentTo(customerToUpdate);
        }

        [Fact]
        public async Task UpdateAsync_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var customerToUpdate = _testCustomers[0];

            // Act
            var result = await _controller.UpdateAsync(999, customerToUpdate);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_WithExistingCustomer_ReturnsNoContent()
        {
            // Arrange
            _mockCustomerRepository.Setup(repo => repo.DeleteAsync(
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAsync(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingCustomer_ReturnsNotFound()
        {
            // Arrange
            _mockCustomerRepository.Setup(repo => repo.DeleteAsync(
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteAsync(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}