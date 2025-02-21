using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;                    // v6.0.0
using Microsoft.AspNetCore.Authorization;          // v6.0.0
using Microsoft.Extensions.Logging;                // v6.0.0
using Backend.Core.Interfaces.Repositories;
using Backend.Core.DTOs.Admin;
using Backend.Core.Entities;

namespace Backend.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling administrative operations including user management,
    /// quick links management, and code type management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdminRole")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the AdminController
        /// </summary>
        public AdminController(
            ILogger<AdminController> logger,
            IUserRepository userRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Retrieves all users in the system
        /// </summary>
        [HttpGet("users")]
        [Authorize(Policy = "EditUsers")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            try
            {
                _logger.LogInformation("Retrieving all users");
                var users = await _userRepository.GetAllAsync();
                var userDtos = new List<UserDto>();
                
                foreach (var user in users)
                {
                    userDtos.Add(_userRepository.ToDto(user));
                }

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }

        /// <summary>
        /// Retrieves a specific user by ID
        /// </summary>
        [HttpGet("users/{id}")]
        [Authorize(Policy = "EditUsers")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving user with ID: {UserId}", id);
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                return Ok(_userRepository.ToDto(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving the user");
            }
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        [HttpPost("users")]
        [Authorize(Policy = "EditUsers")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating new user with email: {Email}", userDto.Email);
                
                var existingUser = await _userRepository.GetByEmailAsync(userDto.Email);
                if (existingUser != null)
                {
                    return Conflict($"User with email {userDto.Email} already exists");
                }

                var user = _userRepository.FromDto(userDto);
                var createdUser = await _userRepository.CreateAsync(user);
                
                return CreatedAtAction(
                    nameof(GetUser),
                    new { id = createdUser.Id },
                    _userRepository.ToDto(createdUser));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}", userDto.Email);
                return StatusCode(500, "An error occurred while creating the user");
            }
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        [HttpPut("users/{id}")]
        [Authorize(Policy = "EditUsers")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != userDto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                _logger.LogInformation("Updating user with ID: {UserId}", id);

                var existingUser = await _userRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                var user = _userRepository.FromDto(userDto);
                var success = await _userRepository.UpdateAsync(user);

                if (!success)
                {
                    return StatusCode(500, "Failed to update user");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while updating the user");
            }
        }

        /// <summary>
        /// Updates user roles
        /// </summary>
        [HttpPut("users/{id}/roles")]
        [Authorize(Policy = "EditUsers")]
        public async Task<ActionResult> UpdateUserRoles(int id, [FromBody] List<string> roles)
        {
            try
            {
                _logger.LogInformation("Updating roles for user with ID: {UserId}", id);

                var existingUser = await _userRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                var success = await _userRepository.UpdateRolesAsync(id, roles);
                if (!success)
                {
                    return StatusCode(500, "Failed to update user roles");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while updating user roles");
            }
        }

        /// <summary>
        /// Updates user email confirmation status
        /// </summary>
        [HttpPut("users/{id}/confirm-email")]
        [Authorize(Policy = "EditUsers")]
        public async Task<ActionResult> UpdateEmailConfirmation(int id, [FromBody] bool confirmed)
        {
            try
            {
                _logger.LogInformation("Updating email confirmation status for user with ID: {UserId}", id);

                var success = await _userRepository.UpdateEmailConfirmationAsync(id, confirmed);
                if (!success)
                {
                    return NotFound($"User with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email confirmation for user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while updating email confirmation status");
            }
        }

        /// <summary>
        /// Regenerates security stamp for a user
        /// </summary>
        [HttpPost("users/{id}/regenerate-security-stamp")]
        [Authorize(Policy = "EditUsers")]
        public async Task<ActionResult<string>> RegenerateSecurityStamp(int id)
        {
            try
            {
                _logger.LogInformation("Regenerating security stamp for user with ID: {UserId}", id);

                var newStamp = await _userRepository.RegenerateSecurityStampAsync(id);
                if (newStamp == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                return Ok(newStamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating security stamp for user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while regenerating security stamp");
            }
        }
    }
}