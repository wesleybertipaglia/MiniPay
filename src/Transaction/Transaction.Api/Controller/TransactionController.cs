using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shared.Core.Dto;
using Shared.Core.Enum;
using Transaction.Core.Dto;
using Transaction.Core.Interface;

namespace Transaction.Api.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController(ITransactionService transactionService) : ControllerBase
{
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
            var transactions = await transactionService.ListAsync(userId, page, size, type, startDate, endDate);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized();

        try
        {
            var transaction = await transactionService.GetByCodeAsync(code);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransactionRequestDto request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized();

        try
        {
            var userDto = GetUserFromClaims();
            var transaction = await transactionService.CreateAsync(userDto, request);
            return CreatedAtAction(nameof(GetByCode), new { code = transaction.Code }, transaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return Guid.TryParse(userIdClaim?.Value, out var userId) ? userId : Guid.Empty;
    }
    
    private UserDto GetUserFromClaims()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        var nameClaim = User.FindFirst(ClaimTypes.Name);
        var emailClaim = User.FindFirst(ClaimTypes.Email);

        var userId = Guid.TryParse(idClaim?.Value, out var id) ? id : Guid.Empty;
        var name = nameClaim?.Value ?? string.Empty;
        var email = emailClaim?.Value ?? string.Empty;

        return new UserDto(
            Id: userId,
            Email: email,
            Code: string.Empty,
            EmailConfirmed: true,
            Name: name
        );
    }
}
