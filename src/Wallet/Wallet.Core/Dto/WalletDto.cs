using Shared.Core.Enum;
using Wallet.Core.Enum;

namespace Wallet.Core.Dto;

public record WalletDto
(
    Guid Id,
    string Code,
    Guid UserId,
    decimal Balance,
    Country Country,
    Currency Currency,
    WalletType Type
);