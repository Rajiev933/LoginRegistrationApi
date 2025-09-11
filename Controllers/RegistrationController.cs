using LoginRegistrationApi.Models;
using LoginRegistrationApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegistrationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly RegistrationRepo _registrationRepo;
        public RegistrationController(ILogger<RegistrationController> logger, RegistrationRepo registrationRepo)
        {
            _logger = logger;
            _registrationRepo = registrationRepo;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Registration([FromBody] UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Here you would typically add code to save the user to a database
            await _registrationRepo.RegisterUserAsync(user);

            return Ok("Registration successful");
        }
    }
}
