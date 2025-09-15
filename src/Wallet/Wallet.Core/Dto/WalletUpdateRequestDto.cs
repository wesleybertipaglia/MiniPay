using Shared.Core.Enum;
using Wallet.Core.Enum;

namespace Wallet.Core.Dto;

public record WalletUpdateRequestDto
(
    Country? Country,
    Currency? Currency,
    WalletType? Type
);