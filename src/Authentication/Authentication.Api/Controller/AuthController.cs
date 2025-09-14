using Authentication.Core.Dto;
using Authentication.Core.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequestDto signInRequestDto)
    {
        try
        {
            var tokenDto = await authService.SignIn(signInRequestDto);
            return Ok(tokenDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"SignIn failed for email: {signInRequestDto.Email}");
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto signUpRequestDto)
    {
        try
        {
            var tokenDto = await authService.SignUp(signUpRequestDto);
            return CreatedAtAction(nameof(SignIn), new { email = signUpRequestDto.Email }, tokenDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"SignUp failed for email: {signUpRequestDto.Email}");
            return BadRequest(new { message = ex.Message });
        }
    }
}