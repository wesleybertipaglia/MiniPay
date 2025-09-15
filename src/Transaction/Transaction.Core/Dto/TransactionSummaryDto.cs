using Shared.Core.Enum;

namespace Transaction.Core.Dto;

public record TransactionSummaryDto
(
    string Code,
    string? TargetWalletCode,
    decimal Amount,
    TransactionType Type,
    TransactionStatus Status
);