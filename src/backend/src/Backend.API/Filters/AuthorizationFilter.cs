using Backend.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Backend.API.Filters
{
    /// <summary>
    /// Enhanced action filter that provides robust role-based and permission-based authorization 
    /// with caching, detailed logging, and security optimizations
    /// </summary>
    public class AuthorizationFilter : IAuthorizationFilter
    {
        private readonly string[] _requiredRoles;
        private readonly string[] _requiredPermissions;
        private readonly ILogger<AuthorizationFilter> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly AuthorizationOptions _options;

        private const string ROLE_CACHE_KEY_PREFIX = "auth_role_";
        private const string PERMISSION_CACHE_KEY_PREFIX = "auth_perm_";
        private const int CACHE_DURATION_MINUTES = 10;

        /// <summary>
        /// Initializes a new instance of the AuthorizationFilter
        /// </summary>
        /// <param name="requiredRoles">Array of required roles for authorization</param>
        /// <param name="requiredPermissions">Array of required permissions for authorization</param>
        /// <param name="logger">Logger instance for authorization tracking</param>
        /// <param name="memoryCache">Memory cache service for caching authorization results</param>
        /// <param name="options">Authorization configuration options</param>
        public AuthorizationFilter(
            string[] requiredRoles,
            string[] requiredPermissions,
            ILogger<AuthorizationFilter> logger,
            IMemoryCache memoryCache,
            AuthorizationOptions options)
        {
            _requiredRoles = requiredRoles ?? Array.Empty<string>();
            _requiredPermissions = requiredPermissions ?? Array.Empty<string>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Executes the authorization logic with caching and detailed logging
        /// </summary>
        /// <param name="context">The authorization filter context</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                // Check if user is authenticated
                if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning("Authorization failed: User is not authenticated");
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var claims = context.HttpContext.User.Claims;
                var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("Authorization failed: User ID claim not found");
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Perform role validation if required
                if (_requiredRoles.Any() && !ValidateRoles(claims))
                {
                    _logger.LogWarning("Authorization failed: User {UserId} lacks required roles", userId);
                    context.Result = new ForbidResult();
                    return;
                }

                // Perform permission validation if required
                if (_requiredPermissions.Any() && !ValidatePermissions(claims))
                {
                    _logger.LogWarning("Authorization failed: User {UserId} lacks required permissions", userId);
                    context.Result = new ForbidResult();
                    return;
                }

                _logger.LogInformation("Authorization successful for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during authorization");
                context.Result = new StatusCodeResult(500);
            }
        }

        /// <summary>
        /// Validates user roles with caching support
        /// </summary>
        /// <param name="claims">User claims</param>
        /// <returns>True if user has required roles</returns>
        private bool ValidateRoles(IEnumerable<Claim> claims)
        {
            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var cacheKey = $"{ROLE_CACHE_KEY_PREFIX}{userId}";

            if (_memoryCache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            var userRoles = claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var hasRequiredRoles = _requiredRoles.All(requiredRole =>
                userRoles.Any(userRole => 
                    string.Equals(userRole, requiredRole, StringComparison.OrdinalIgnoreCase)));

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                .SetPriority(CacheItemPriority.High);

            _memoryCache.Set(cacheKey, hasRequiredRoles, cacheOptions);

            _logger.LogDebug("Role validation for user {UserId}: {Result}", userId, hasRequiredRoles);
            return hasRequiredRoles;
        }

        /// <summary>
        /// Validates user permissions with caching support
        /// </summary>
        /// <param name="claims">User claims</param>
        /// <returns>True if user has required permissions</returns>
        private bool ValidatePermissions(IEnumerable<Claim> claims)
        {
            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var cacheKey = $"{PERMISSION_CACHE_KEY_PREFIX}{userId}";

            if (_memoryCache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            var userPermissions = claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToList();

            var hasRequiredPermissions = _requiredPermissions.All(requiredPermission =>
                userPermissions.Any(userPermission => 
                    string.Equals(userPermission, requiredPermission, StringComparison.OrdinalIgnoreCase)));

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                .SetPriority(CacheItemPriority.High);

            _memoryCache.Set(cacheKey, hasRequiredPermissions, cacheOptions);

            _logger.LogDebug("Permission validation for user {UserId}: {Result}", userId, hasRequiredPermissions);
            return hasRequiredPermissions;
        }
    }

    /// <summary>
    /// Configuration options for the authorization filter
    /// </summary>
    public class AuthorizationOptions
    {
        /// <summary>
        /// Gets or sets whether to use case-sensitive role matching
        /// </summary>
        public bool CaseSensitiveRoleMatching { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to use case-sensitive permission matching
        /// </summary>
        public bool CaseSensitivePermissionMatching { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to enable caching of authorization results
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Gets or sets the duration in minutes for cached authorization results
        /// </summary>
        public int CacheDurationMinutes { get; set; } = 10;
    }
}