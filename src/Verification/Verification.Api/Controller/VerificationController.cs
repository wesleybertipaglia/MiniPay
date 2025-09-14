using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Verification.Core.Interface;

namespace Verification.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class VerificationController(IVerificationCodeService verificationCodeService) : ControllerBase
{
    [HttpGet("confirm-email/{code}")]
    public async Task<IActionResult> ValidateEmailCode(string code)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized();
        
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Invalid parameters.");
        }

        try
        {
            await verificationCodeService.ValidateAsync(userId, code);
            return Ok(new { Message = "Verification code successfully validated." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Internal error." });
        }
    }
    
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return Guid.TryParse(userIdClaim?.Value, out var userId) ? userId : Guid.Empty;
    }
}