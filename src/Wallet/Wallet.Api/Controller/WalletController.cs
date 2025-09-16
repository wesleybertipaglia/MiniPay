using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Wallet.Core.Dto;
using Wallet.Core.Interface;

namespace Wallet.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class WalletController(IWalletService walletService, ILogger<WalletController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByUserId()
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized();
        
        var wallet = await walletService.GetByUserIdAsync(userId);
        if (wallet is not null)
            return Ok(wallet);

        logger.LogWarning("GET Wallet failed: wallet for user {UserId} not found", userId);
        return NotFound(new { message = $"Wallet for user {userId} not found." });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] WalletUpdateRequestDto requestDto)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized();

        try
        {
            var updated = await walletService.UpdateAsync(requestDto, userId);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update wallet for user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }    
    
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return Guid.TryParse(userIdClaim?.Value, out var userId) ? userId : Guid.Empty;
    }
}