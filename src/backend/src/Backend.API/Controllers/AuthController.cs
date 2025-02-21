using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Backend.Core.DTOs.Admin;
using Backend.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Backend.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling authentication and authorization operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthController(
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        /// <summary>
        /// Authenticates a user and generates JWT tokens
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Email}", request.Email);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed - User not found: {Email}", request.Email);
                    return Unauthorized("Invalid credentials");
                }

                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Login failed - Email not confirmed: {Email}", request.Email);
                    return Unauthorized("Email not confirmed");
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed - Invalid password: {Email}", request.Email);
                    return Unauthorized("Invalid credentials");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var token = GenerateJwtToken(claims);
                var refreshToken = GenerateRefreshToken();

                await _userRepository.UpdateAsync(new User 
                { 
                    Id = user.Id,
                    LastLoginAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString()
                });

                _logger.LogInformation("Login successful for user: {Email}", request.Email);

                return Ok(new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresIn = int.Parse(_configuration["Jwt:ExpiryInMinutes"]) * 60,
                    User = _userRepository.ToDto(user)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", request.Email);
                return StatusCode(500, "An error occurred during login");
            }
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID not found in token");
                }

                await _userRepository.UpdateSecurityStampAsync(int.Parse(userId));
                _logger.LogInformation("Logout successful for user ID: {UserId}", userId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, "An error occurred during logout");
            }
        }

        /// <summary>
        /// Refreshes the JWT token using a valid refresh token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(request.Token);
                if (principal == null)
                {
                    return BadRequest("Invalid token");
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID not found in token");
                }

                var user = await _userRepository.GetByIdAsync(int.Parse(userId));
                if (user == null)
                {
                    return BadRequest("User not found");
                }

                var newToken = GenerateJwtToken(principal.Claims.ToList());
                var newRefreshToken = GenerateRefreshToken();

                _logger.LogInformation("Token refresh successful for user ID: {UserId}", userId);

                return Ok(new LoginResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = int.Parse(_configuration["Jwt:ExpiryInMinutes"]) * 60,
                    User = _userRepository.ToDto(user)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, "An error occurred during token refresh");
            }
        }

        private string GenerateJwtToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public UserDto User { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}