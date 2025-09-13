using LoginRegistrationApi.DTOs;
using LoginRegistrationApi.Models;
using LoginRegistrationApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LoginRegistrationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly RegistrationRepo _registrationRepo;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        public RegistrationController(ILogger<RegistrationController> logger, RegistrationRepo registrationRepo, IConfiguration configuration, AppDbContext context)
        {
            _logger = logger;
            _registrationRepo = registrationRepo;
            _configuration = configuration;
            _context = context;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Registration([FromBody] UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _registrationRepo.RegisterUserAsync(user);

            return Ok("Registration successful");
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate user credentials
            var loggedInUser = await _registrationRepo.GetUserAsync(user.username, user.password);
            if (loggedInUser == null)
            {
                return Unauthorized("Invalid username or password");
            }

            // Generate JWT
            var token = GenerateToken(loggedInUser.Username);
            var refreshToken = GenerateRefreshToken();

            loggedInUser.RefreshToken = refreshToken;
            loggedInUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _registrationRepo.UpdateUserAsync(loggedInUser);

            // Example expiry (set same as your GenerateToken method)
            var expiresIn = DateTime.UtcNow.AddMinutes(60);

            // Return structured response
            return Ok(new
            {
                message = "Login successful",
                token = token,
                expiresIn = expiresIn,
                refreshToken = refreshToken,
                user = new
                {
                    id = loggedInUser.Id,
                    username = loggedInUser.Username,
                    email = loggedInUser.Email,
                    fullName = loggedInUser.FullName
                }
            });
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request)
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null) return BadRequest("Invalid access token");

            var username = principal.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Unauthorized("Invalid refresh token");
            }

            // Generate new tokens
            var newJwtToken = GenerateToken(user.Username);
            var newRefreshToken = GenerateRefreshToken();

            // Update DB with new refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = newJwtToken,
                refreshToken = newRefreshToken
            });
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false // 👈 Ignore expiration here
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            return (securityToken is JwtSecurityToken jwtSecurityToken &&
                    jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase)) ? principal : null;
        }


        private string GenerateToken(string username)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: creds
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

    }
}
