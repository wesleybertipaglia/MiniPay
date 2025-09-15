using Wallet.Core.Dto;

namespace Wallet.Core.Interface;

public interface IWalletService
{
    Task<WalletDto?> GetByIdAsync(Guid id);
    Task<WalletDto?> GetByCodeAsync(string code);
    Task<WalletDto> CreateAsync(WalletDto walletDto);
    Task<WalletDto> UpdateAsync(Guid id, WalletUpdateRequestDto walletDto);
    Task DeleteAsync(WalletDto walletDto);
}