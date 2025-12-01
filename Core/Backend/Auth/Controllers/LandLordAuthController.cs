using Microsoft.AspNetCore.Mvc;
using RentMaster.Core.Backend.Auth.Interface;
using RentMaster.Core.Backend.Auth.Types.enums;
using RentMaster.Core.Backend.Auth.Types.Response;
using LoginRequest = RentMaster.Core.Backend.Auth.Types.Request.LoginRequest;

[ApiController]
[Route("landlord/api/auth")]
public class LandLordAuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public LandLordAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var response = await _authService.LoginAsync(model.Gmail, model.Password, UserTypes.LandLord);
        if (response == null)
            return Unauthorized(new { message = "Invalid Gmail or Password" });

        return Ok(response);
    }
}