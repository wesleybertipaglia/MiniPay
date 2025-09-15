using Microsoft.AspNetCore.Mvc;
using Wallet.Core.Dto;
using Wallet.Core.Interface;

namespace Wallet.Api.Controller;


[ApiController]
[Route("api/[controller]")]
public class WalletController(IWalletService walletService, ILogger<WalletController> logger) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var wallet = await walletService.GetByIdAsync(id);
        if (wallet is not null) return Ok(wallet);
        logger.LogWarning("GET Wallet by ID failed: {WalletId} not found", id);
        return NotFound(new { message = $"Wallet {id} not found." });

    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var wallet = await walletService.GetByCodeAsync(code);
        if (wallet is not null) return Ok(wallet);
        logger.LogWarning("GET Wallet by Code failed: {WalletCode} not found", code);
        return NotFound(new { message = $"Wallet with code '{code}' not found." });

    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] WalletUpdateRequestDto updateDto)
    {
        try
        {
            var updated = await walletService.UpdateAsync(id, updateDto);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update wallet {WalletId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}