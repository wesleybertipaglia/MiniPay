using Wallet.Core.Dto;

namespace Wallet.Core.Mapper;

public static class WalletMapper
{
    public static WalletDto ToDto(this Model.Wallet wallet)
    {
        return new WalletDto
        (
            Id: wallet.Id,
            Code:  wallet.Code,
            UserId:   wallet.UserId,
            Balance:  wallet.Balance,
            Country: wallet.Country,
            Currency: wallet.Currency,
            Type: wallet.Type
        );
    }
}