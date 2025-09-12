using LoginRegistrationApi.DTOs;
using LoginRegistrationApi.Models;
using LoginRegistrationApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        public RegistrationController(ILogger<RegistrationController> logger, RegistrationRepo registrationRepo, IConfiguration configuration)
        {
            _logger = logger;
            _registrationRepo = registrationRepo;
            _configuration = configuration;

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
            var isValid = await _registrationRepo.LoginUserAsync(user.username, user.password);
            if (!isValid)
                return Unauthorized("Invalid username or password");
            var token = GenerateToken(user.username);

            return Ok(new { message= "Login successful", Token= token});
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
    }
}
