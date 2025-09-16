using System.ComponentModel.DataAnnotations;
using Shared.Core.Enum;

namespace Transaction.Core.Dto;

public record TransactionRequestDto
(
    string? TargetWalletCode,
    string? TargetTransactionCode,
    TransactionType Type,

    [Required, MinLength(3)]
    string Description,

    [Range(0.01, double.MaxValue)]
    decimal Amount
);
