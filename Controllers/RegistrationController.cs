using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegistrationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        public RegistrationController() { }
        [HttpPost("Register")]
        public ActionResult<string> ActionResult(string username, string password)
        {
            return "Hello " + username;
        }
    }
}
