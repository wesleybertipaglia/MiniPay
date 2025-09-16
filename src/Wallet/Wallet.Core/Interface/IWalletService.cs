using Wallet.Core.Dto;

namespace Wallet.Core.Interface;

public interface IWalletService
{
    Task<WalletDto?> GetByUserIdAsync(Guid id);
    Task<WalletDto> CreateAsync(Guid userId);
    Task<WalletDto> CreateAsync(WalletCreateRequestDto requestDto, Guid userId);
    Task<WalletDto> UpdateAsync(WalletUpdateRequestDto requestDto, Guid userId);
}