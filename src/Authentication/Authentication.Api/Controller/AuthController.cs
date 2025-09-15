using Authentication.Core.Dto;
using Authentication.Core.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequestDto signInRequestDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for SignIn request: {Email}", signInRequestDto.Email);
            return BadRequest(ModelState);
        }

        try
        {
            var tokenDto = await authService.SignIn(signInRequestDto);
            logger.LogInformation("User signed in successfully: {Email}", signInRequestDto.Email);
            return Ok(tokenDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SignIn failed for email: {Email}", signInRequestDto.Email);
            return Unauthorized(new { message = "Invalid credentials." });
        }
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto signUpRequestDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for SignUp request: {Email}", signUpRequestDto.Email);
            return BadRequest(ModelState);
        }

        try
        {
            var tokenDto = await authService.SignUp(signUpRequestDto);
            logger.LogInformation("User signed up successfully: {Email}", signUpRequestDto.Email);
            
            return Ok(tokenDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SignUp failed for email: {Email}", signUpRequestDto.Email);
            return BadRequest(new { message = ex.Message });
        }
    }
}