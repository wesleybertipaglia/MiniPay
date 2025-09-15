using Shared.Core.Enum;

namespace Transaction.Core.Dto;

public record TransactionDetailsDto
(
    Guid Id,
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