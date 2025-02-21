using Backend.Core.Entities;
using Backend.Core.DTOs.Admin;
using Backend.Core.Interfaces.Repositories;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Infrastructure.Repositories
{
    /// <summary>
    /// Implements IUserRepository with enhanced security, caching, and monitoring capabilities
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserRepository> _logger;
        private const string USER_CACHE_KEY = "User_{0}"; // {0} = userId
        private const string EMAIL_CACHE_KEY = "UserEmail_{0}"; // {0} = email
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(15);

        public UserRepository(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<UserRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<User> GetByIdAsync(int id)
        {
            try
            {
                string cacheKey = string.Format(USER_CACHE_KEY, id);
                
                if (_cache.TryGetValue(cacheKey, out User cachedUser))
                {
                    _logger.LogDebug("Cache hit for user ID: {UserId}", id);
                    return cachedUser;
                }

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

                if (user != null)
                {
                    _cache.Set(cacheKey, user, CACHE_DURATION);
                    _logger.LogDebug("User {UserId} cached", id);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", id);
                throw;
            }
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email cannot be empty", nameof(email));

                string cacheKey = string.Format(EMAIL_CACHE_KEY, email.ToLower());

                if (_cache.TryGetValue(cacheKey, out User cachedUser))
                {
                    _logger.LogDebug("Cache hit for user email: {Email}", email);
                    return cachedUser;
                }

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

                if (user != null)
                {
                    _cache.Set(cacheKey, user, CACHE_DURATION);
                    _logger.LogDebug("User with email {Email} cached", email);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> ValidateEmailConfirmationAsync(int userId)
        {
            try
            {
                var user = await GetByIdAsync(userId);
                return user?.EmailConfirmed ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email confirmation for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ValidateSecurityStampAsync(int userId, string securityStamp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(securityStamp))
                    return false;

                var user = await GetByIdAsync(userId);
                return user?.SecurityStamp == securityStamp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating security stamp for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                InvalidateUserCache(userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                await ValidateUserAsync(user);

                user.CreatedAt = DateTime.UtcNow;
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.ConcurrencyStamp = Guid.NewGuid().ToString();

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new user: {UserId}", user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                await ValidateUserAsync(user);

                var existingUser = await _context.Users.FindAsync(user.Id);
                if (existingUser == null) return false;

                if (existingUser.ConcurrencyStamp != user.ConcurrencyStamp)
                {
                    throw new DbUpdateConcurrencyException("User has been modified by another process");
                }

                _context.Entry(existingUser).CurrentValues.SetValues(user);
                existingUser.ConcurrencyStamp = Guid.NewGuid().ToString();

                await _context.SaveChangesAsync();
                InvalidateUserCache(user.Id);

                _logger.LogInformation("Updated user: {UserId}", user.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user?.Id);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false)
        {
            try
            {
                var query = _context.Users.AsNoTracking();
                
                if (!includeInactive)
                    query = query.Where(u => u.IsActive);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                throw;
            }
        }

        public async Task<bool> UpdateRolesAsync(int userId, IEnumerable<string> roles)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.Roles = roles.ToList();
                user.ConcurrencyStamp = Guid.NewGuid().ToString();

                await _context.SaveChangesAsync();
                InvalidateUserCache(userId);

                _logger.LogInformation("Updated roles for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateEmailConfirmationAsync(int userId, bool confirmed)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.EmailConfirmed = confirmed;
                user.ConcurrencyStamp = Guid.NewGuid().ToString();

                await _context.SaveChangesAsync();
                InvalidateUserCache(userId);

                _logger.LogInformation("Updated email confirmation for user: {UserId} to {Confirmed}", userId, confirmed);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email confirmation for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<string> RegenerateSecurityStampAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return null;

                user.SecurityStamp = Guid.NewGuid().ToString();
                user.ConcurrencyStamp = Guid.NewGuid().ToString();

                await _context.SaveChangesAsync();
                InvalidateUserCache(userId);

                _logger.LogInformation("Regenerated security stamp for user: {UserId}", userId);
                return user.SecurityStamp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating security stamp for user: {UserId}", userId);
                throw;
            }
        }

        public UserDto ToDto(User user)
        {
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Roles = user.Roles?.ToList() ?? new List<string>(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                ReceiveEmails = user.ReceiveEmails
            };
        }

        public User FromDto(UserDto userDto)
        {
            if (userDto == null) return null;

            return new User
            {
                Id = userDto.Id,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                EmailConfirmed = userDto.EmailConfirmed,
                Roles = userDto.Roles?.ToList() ?? new List<string>(),
                IsActive = userDto.IsActive,
                CreatedAt = userDto.CreatedAt,
                LastLoginAt = userDto.LastLoginAt,
                ReceiveEmails = userDto.ReceiveEmails
            };
        }

        private async Task ValidateUserAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.FirstName))
                throw new ArgumentException("First name is required", nameof(user));

            if (string.IsNullOrWhiteSpace(user.LastName))
                throw new ArgumentException("Last name is required", nameof(user));

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("Email is required", nameof(user));

            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower() && u.Id != user.Id);

            if (existingUser != null)
                throw new InvalidOperationException($"Email {user.Email} is already in use");
        }

        private void InvalidateUserCache(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                _cache.Remove(string.Format(USER_CACHE_KEY, userId));
                _cache.Remove(string.Format(EMAIL_CACHE_KEY, user.Email.ToLower()));
            }
        }
    }
}