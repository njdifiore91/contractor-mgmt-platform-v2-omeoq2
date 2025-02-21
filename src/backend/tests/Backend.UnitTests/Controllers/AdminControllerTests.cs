using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;  // v2.2.0
using Moq;                                          // v4.16.1
using Backend.API.Controllers;
using Backend.Core.DTOs.Admin;
using Backend.Core.Entities;
using Backend.Core.Interfaces.Repositories;

namespace Backend.UnitTests.Controllers
{
    [TestClass]
    public class AdminControllerTests
    {
        private Mock<ILogger<AdminController>> _mockLogger;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IAuthorizationService> _mockAuthService;
        private AdminController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockLogger = new Mock<ILogger<AdminController>>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuthService = new Mock<IAuthorizationService>();

            _controller = new AdminController(
                _mockLogger.Object,
                _mockUserRepository.Object);
        }

        [TestMethod]
        public async Task GetUsers_WithValidPermission_ReturnsUsers()
        {
            // Arrange
            var testUsers = new List<User>
            {
                new User 
                { 
                    Id = 1, 
                    FirstName = "John", 
                    LastName = "Doe", 
                    Email = "john@example.com",
                    EmailConfirmed = true,
                    IsActive = true
                },
                new User 
                { 
                    Id = 2, 
                    FirstName = "Jane", 
                    LastName = "Smith", 
                    Email = "jane@example.com",
                    EmailConfirmed = false,
                    IsActive = true
                }
            };

            var testUserDtos = new List<UserDto>
            {
                new UserDto 
                { 
                    Id = 1, 
                    FirstName = "John", 
                    LastName = "Doe", 
                    Email = "john@example.com",
                    EmailConfirmed = true,
                    IsActive = true
                },
                new UserDto 
                { 
                    Id = 2, 
                    FirstName = "Jane", 
                    LastName = "Smith", 
                    Email = "jane@example.com",
                    EmailConfirmed = false,
                    IsActive = true
                }
            };

            _mockUserRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(testUsers);

            _mockUserRepository.Setup(r => r.ToDto(It.IsAny<User>()))
                .Returns<User>(u => testUserDtos.Find(dto => dto.Id == u.Id));

            // Act
            var result = await _controller.GetUsers();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            var returnedUsers = okResult.Value as IEnumerable<UserDto>;
            CollectionAssert.AreEqual(testUserDtos, new List<UserDto>(returnedUsers));
        }

        [TestMethod]
        public async Task GetUser_WithValidId_ReturnsUser()
        {
            // Arrange
            int userId = 1;
            var testUser = new User 
            { 
                Id = userId, 
                FirstName = "John", 
                LastName = "Doe", 
                Email = "john@example.com" 
            };
            var testUserDto = new UserDto 
            { 
                Id = userId, 
                FirstName = "John", 
                LastName = "Doe", 
                Email = "john@example.com" 
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(testUser);
            _mockUserRepository.Setup(r => r.ToDto(testUser))
                .Returns(testUserDto);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(testUserDto, okResult.Value);
        }

        [TestMethod]
        public async Task GetUser_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int userId = 999;
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task CreateUser_WithValidData_ReturnsCreatedUser()
        {
            // Arrange
            var newUserDto = new UserDto 
            { 
                FirstName = "New", 
                LastName = "User", 
                Email = "new@example.com",
                Roles = new List<string> { "User" }
            };

            var newUser = new User 
            { 
                FirstName = "New", 
                LastName = "User", 
                Email = "new@example.com" 
            };

            var createdUser = new User 
            { 
                Id = 1, 
                FirstName = "New", 
                LastName = "User", 
                Email = "new@example.com" 
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(newUserDto.Email))
                .ReturnsAsync((User)null);
            _mockUserRepository.Setup(r => r.FromDto(newUserDto))
                .Returns(newUser);
            _mockUserRepository.Setup(r => r.CreateAsync(newUser))
                .ReturnsAsync(createdUser);
            _mockUserRepository.Setup(r => r.ToDto(createdUser))
                .Returns(newUserDto);

            // Act
            var result = await _controller.CreateUser(newUserDto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.AreEqual(newUserDto, createdResult.Value);
        }

        [TestMethod]
        public async Task CreateUser_WithExistingEmail_ReturnsConflict()
        {
            // Arrange
            var newUserDto = new UserDto 
            { 
                FirstName = "New", 
                LastName = "User", 
                Email = "existing@example.com" 
            };

            var existingUser = new User 
            { 
                Id = 1, 
                Email = "existing@example.com" 
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(newUserDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _controller.CreateUser(newUserDto);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
        }

        [TestMethod]
        public async Task UpdateUser_WithValidData_ReturnsNoContent()
        {
            // Arrange
            int userId = 1;
            var updateUserDto = new UserDto 
            { 
                Id = userId, 
                FirstName = "Updated", 
                LastName = "User", 
                Email = "updated@example.com" 
            };

            var existingUser = new User { Id = userId };
            var updatedUser = new User 
            { 
                Id = userId, 
                FirstName = "Updated", 
                LastName = "User", 
                Email = "updated@example.com" 
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);
            _mockUserRepository.Setup(r => r.FromDto(updateUserDto))
                .Returns(updatedUser);
            _mockUserRepository.Setup(r => r.UpdateAsync(updatedUser))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUser(userId, updateUserDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task UpdateUserRoles_WithValidData_ReturnsNoContent()
        {
            // Arrange
            int userId = 1;
            var roles = new List<string> { "Admin", "User" };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId });
            _mockUserRepository.Setup(r => r.UpdateRolesAsync(userId, roles))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUserRoles(userId, roles);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task UpdateEmailConfirmation_WithValidData_ReturnsNoContent()
        {
            // Arrange
            int userId = 1;
            bool confirmed = true;

            _mockUserRepository.Setup(r => r.UpdateEmailConfirmationAsync(userId, confirmed))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateEmailConfirmation(userId, confirmed);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task RegenerateSecurityStamp_WithValidUser_ReturnsNewStamp()
        {
            // Arrange
            int userId = 1;
            string newStamp = Guid.NewGuid().ToString();

            _mockUserRepository.Setup(r => r.RegenerateSecurityStampAsync(userId))
                .ReturnsAsync(newStamp);

            // Act
            var result = await _controller.RegenerateSecurityStamp(userId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(newStamp, okResult.Value);
        }
    }
}