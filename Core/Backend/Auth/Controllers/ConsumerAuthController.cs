using Microsoft.AspNetCore.Mvc;
using RentMaster.Core.Backend.Auth.Interface;
using RentMaster.Core.Backend.Auth.Types.enums;
using LoginRequest = RentMaster.Core.Backend.Auth.Types.Request.LoginRequest;

namespace RentMaster.Controllers
{
    [ApiController]
    [Route("consumer/api/auth")]
    public class ConsumerAuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public ConsumerAuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var response = await _authService.LoginAsync(model.Gmail, model.Password, UserTypes.Consumer);
            if (response == null)
                return Unauthorized(new { message = "Invalid Gmail or Password" });

            return Ok(response);
        }
    }
}