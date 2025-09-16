using Microsoft.Extensions.Logging;
using Shared.Core.Interface;
using Wallet.Core.Dto;
using Wallet.Core.Interface;
using Wallet.Core.Mapper;
using Wallet.Core.Helper;

namespace Wallet.Application.Service;

public class WalletService(
    IWalletRepository walletRepository,
    ILogger<WalletService> logger,
    ICacheService cacheService)
    : IWalletService
{
    public async Task<WalletDto?> GetByUserIdAsync(Guid userId)
    {
        var cachedWallet = await GetWalletCacheByUserIdAsync(userId);
        if (cachedWallet != null)
        {
            logger.LogInformation("Retrieved wallet of user {UserId} from cache", userId);
            return cachedWallet;
        }

        var wallet = await walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            logger.LogWarning("Wallet not found with UserId: {UserId}", userId);
            return null;
        }

        var mappedWallet = wallet.Map();
        await CacheWalletAsync(mappedWallet);

        logger.LogInformation("Wallet {WalletId} fetched from DB and cached", wallet.Id);
        return mappedWallet;
    }

    public async Task<WalletDto> CreateAsync(Guid userId)
    {
        var wallet = new Core.Model.Wallet(userId);
        await walletRepository.CreateAsync(wallet);
        await InvalidateWalletCacheAsync(wallet);

        logger.LogInformation("Wallet {WalletId} created", wallet.Id);
        return wallet.Map();
    }

    public async Task<WalletDto> CreateAsync(WalletCreateRequestDto requestDto, Guid userId)
    {
        var wallet = requestDto.Map(userId);
        await walletRepository.CreateAsync(wallet);
        await InvalidateWalletCacheAsync(wallet);

        logger.LogInformation("Wallet {WalletId} created", wallet.Id);
        return wallet.Map();
    }

    public async Task<WalletDto> UpdateAsync(WalletUpdateRequestDto requestDto, Guid userId)
    {
        var wallet = await walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            logger.LogWarning("Cannot update: wallet of user {UserId} not found", userId);
            throw new Exception($"Wallet of user {userId} not found.");
        }

        wallet.Country = requestDto.Country ?? wallet.Country;
        wallet.Currency = requestDto.Currency ?? wallet.Currency;
        wallet.Type = requestDto.Type ?? wallet.Type;

        var updatedWallet = await walletRepository.UpdateAsync(wallet);
        await InvalidateWalletCacheAsync(updatedWallet);

        logger.LogInformation("Wallet {WalletId} updated", updatedWallet.Id);
        return updatedWallet.Map();
    }

    private async Task CacheWalletAsync(WalletDto walletDto)
    {
        var expiration = TimeSpan.FromMinutes(5);
        await cacheService.SetAsync(WalletCacheKeys.GetWalletByUserIdKey(walletDto.UserId), walletDto, expiration);

        logger.LogDebug("Wallet {WalletId} cached with expiration {Expiration}", walletDto.Id, expiration);
    }

    private async Task<WalletDto?> GetWalletCacheByUserIdAsync(Guid userId)
    {
        var key = WalletCacheKeys.GetWalletByUserIdKey(userId);
        return await cacheService.GetAsync<WalletDto>(key);
    }

    private async Task InvalidateWalletCacheAsync(Core.Model.Wallet wallet)
    {
        await cacheService.RemoveAsync(WalletCacheKeys.GetWalletByUserIdKey(wallet.UserId));

        logger.LogDebug("Cache invalidated for wallet {WalletId} and code {WalletCode}", wallet.Id, wallet.Code);
    }
}
