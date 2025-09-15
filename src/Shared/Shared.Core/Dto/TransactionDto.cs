using Shared.Core.Enum;

namespace Shared.Core.Dto;

public record TransactionDto
(
    Guid Id,
    string Code,
    Guid WalletOriginId,
    Guid WalletRecipientId,
    string Description,
    decimal Amount,
    Currency Currency,
    TransactionType TransactionType
);