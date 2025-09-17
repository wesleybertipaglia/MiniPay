namespace Shared.Core.Dto;

public record TransactionEventDto
(
    UserDto UserDto,
    TransactionDto TransactionDto,
    bool Success
);