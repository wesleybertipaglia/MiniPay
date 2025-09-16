using Shared.Core.Enum;

namespace Shared.Core.Dto;

public record TransactionDto
(
    string Code,
    Guid UserId,
    string? TargetTransactionCode,
    string? TargetWalletCode,
    string Description,
    decimal Amount,
    TransactionType Type,
    TransactionStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);