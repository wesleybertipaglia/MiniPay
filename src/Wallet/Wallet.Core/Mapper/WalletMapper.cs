using Wallet.Core.Dto;

namespace Wallet.Core.Mapper;

public static class WalletMapper
{
    public static WalletDto Map(this Model.Wallet wallet)
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
    
    public static Model.Wallet Map(this WalletDto walletDto)
    {
        return new Model.Wallet
        (
            userId:  walletDto.UserId,
            balance: walletDto.Balance,
            country:  walletDto.Country,
            currency:  walletDto.Currency,
            type:  walletDto.Type
        );
    }
}