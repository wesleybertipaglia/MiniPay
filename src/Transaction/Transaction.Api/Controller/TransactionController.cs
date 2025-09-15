using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shared.Core.Enum;
using Transaction.Core.Dto;
using Transaction.Core.Interface;

namespace Transaction.Api.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController(ITransactionService transactionService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransactionRequestDto request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized();

        try
        {
            var transaction = await transactionService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] TransactionType? type = null,
        [FromQuery(Name = "start_date")] DateTime? startDate = null,
        [FromQuery(Name = "end_date")] DateTime? endDate = null)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized();

        try
        {
            var walletId = userId;

            var transactions = await transactionService.ListAsync(walletId, page, size, type, startDate, endDate);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized();

        try
        {
            var transaction = await transactionService.GetByIdAsync(id);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return Guid.TryParse(userIdClaim?.Value, out var userId) ? userId : Guid.Empty;
    }
}
