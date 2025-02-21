using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Core.DTOs.Admin;
using Backend.Core.Entities;

namespace Backend.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for managing user entities with enhanced security features
    /// and comprehensive user management capabilities.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves a user by their unique identifier with security validation
        /// </summary>
        /// <param name="id">The unique identifier of the user</param>
        /// <returns>The user entity if found and valid, null otherwise</returns>
        Task<User> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves a user by their email address with security validation
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <returns>The user entity if found and valid, null otherwise</returns>
        Task<User> GetByEmailAsync(string email);

        /// <summary>
        /// Validates whether a user's email is confirmed
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>True if email is confirmed, false otherwise</returns>
        Task<bool> ValidateEmailConfirmationAsync(int userId);

        /// <summary>
        /// Validates a user's security stamp for session management
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="securityStamp">The security stamp to validate</param>
        /// <returns>True if security stamp is valid, false otherwise</returns>
        Task<bool> ValidateSecurityStampAsync(int userId, string securityStamp);

        /// <summary>
        /// Updates the last login timestamp for a user
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>True if update successful, false otherwise</returns>
        Task<bool> UpdateLastLoginAsync(int userId);

        /// <summary>
        /// Creates a new user in the system
        /// </summary>
        /// <param name="user">The user entity to create</param>
        /// <returns>The created user with assigned Id</returns>
        Task<User> CreateAsync(User user);

        /// <summary>
        /// Updates an existing user's information
        /// </summary>
        /// <param name="user">The user entity with updated information</param>
        /// <returns>True if update successful, false otherwise</returns>
        Task<bool> UpdateAsync(User user);

        /// <summary>
        /// Retrieves all users with optional filtering
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive users</param>
        /// <returns>List of user entities matching the criteria</returns>
        Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false);

        /// <summary>
        /// Updates a user's roles
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="roles">The new list of roles</param>
        /// <returns>True if update successful, false otherwise</returns>
        Task<bool> UpdateRolesAsync(int userId, IEnumerable<string> roles);

        /// <summary>
        /// Updates a user's email confirmation status
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="confirmed">The new confirmation status</param>
        /// <returns>True if update successful, false otherwise</returns>
        Task<bool> UpdateEmailConfirmationAsync(int userId, bool confirmed);

        /// <summary>
        /// Generates a new security stamp for a user
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>The new security stamp if successful, null otherwise</returns>
        Task<string> RegenerateSecurityStampAsync(int userId);

        /// <summary>
        /// Converts a User entity to a UserDto
        /// </summary>
        /// <param name="user">The user entity to convert</param>
        /// <returns>The converted UserDto</returns>
        UserDto ToDto(User user);

        /// <summary>
        /// Converts a UserDto to a User entity
        /// </summary>
        /// <param name="userDto">The UserDto to convert</param>
        /// <returns>The converted User entity</returns>
        User FromDto(UserDto userDto);
    }
}