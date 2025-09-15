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
    public async Task<WalletDto?> GetByIdAsync(Guid id)
    {
        var cachedWallet = await GetWalletCacheByIdAsync(id);
        if (cachedWallet != null)
        {
            logger.LogInformation("Retrieved wallet {WalletId} from cache", id);
            return cachedWallet;
        }

        var wallet = await walletRepository.GetByIdAsync(id);
        if (wallet is null)
        {
            logger.LogWarning("Wallet not found with ID: {WalletId}", id);
            return null;
        }

        var mappedWallet = wallet.Map();
        await CacheWalletAsync(mappedWallet);

        logger.LogInformation("Wallet {WalletId} fetched from DB and cached", id);
        return mappedWallet;
    }

    public async Task<WalletDto?> GetByCodeAsync(string code)
    {
        var cachedWallet = await GetWalletCacheByCodeAsync(code);
        if (cachedWallet != null)
        {
            logger.LogInformation("Retrieved wallet {WalletCode} from cache", code);
            return cachedWallet;
        }

        var wallet = await walletRepository.GetByCodeAsync(code);
        if (wallet is null)
        {
            logger.LogWarning("Wallet not found with code: {WalletCode}", code);
            return null;
        }

        var mappedWallet = wallet.Map();
        await CacheWalletAsync(mappedWallet);

        logger.LogInformation("Wallet {WalletCode} fetched from DB and cached", code);
        return mappedWallet;
    }

    public async Task<WalletDto> CreateAsync(WalletDto walletDto)
    {
        var existing = await walletRepository.GetByIdAsync(walletDto.Id);
        if (existing != null)
        {
            logger.LogWarning("Cannot create wallet: wallet {WalletId} already exists", walletDto.Id);
            throw new Exception($"Wallet {walletDto.Id} already exists.");
        }

        var wallet = walletDto.Map();
        await walletRepository.CreateAsync(wallet);
        await InvalidateWalletCacheAsync(wallet);

        logger.LogInformation("Wallet {WalletId} created", wallet.Id);
        return wallet.Map();
    }

    public async Task<WalletDto> UpdateAsync(Guid id, WalletUpdateRequestDto walletDto)
    {
        var wallet = await walletRepository.GetByIdAsync(id);
        if (wallet is null)
        {
            logger.LogWarning("Cannot update: wallet {WalletId} not found", id);
            throw new Exception($"Wallet {id} not found.");
        }

        if (walletDto.Country != null) wallet.Country = walletDto.Country.GetValueOrDefault();
        if (walletDto.Currency != null) wallet.Currency = walletDto.Currency.GetValueOrDefault();
        if (walletDto.Type != null) wallet.Type = walletDto.Type.GetValueOrDefault();

        var updatedWallet = await walletRepository.UpdateAsync(wallet);
        await InvalidateWalletCacheAsync(updatedWallet);

        logger.LogInformation("Wallet {WalletId} updated", updatedWallet.Id);
        return updatedWallet.Map();
    }

    public async Task DeleteAsync(WalletDto walletDto)
    {
        var wallet = await walletRepository.GetByIdAsync(walletDto.Id);
        if (wallet is null)
        {
            logger.LogWarning("Cannot delete: wallet {WalletId} not found", walletDto.Id);
            throw new Exception($"Wallet {walletDto.Id} not found.");
        }

        await walletRepository.DeleteAsync(wallet);
        await InvalidateWalletCacheAsync(wallet);

        logger.LogInformation("Wallet {WalletId} deleted", wallet.Id);
    }

    private async Task CacheWalletAsync(WalletDto walletDto)
    {
        var expiration = TimeSpan.FromMinutes(5);
        await cacheService.SetAsync(CacheKeys.GetWalletByIdKey(walletDto.Id), walletDto, expiration);
        await cacheService.SetAsync(CacheKeys.GetWalletByCodeKey(walletDto.Code), walletDto, expiration);

        logger.LogDebug("Wallet {WalletId} cached with expiration {Expiration}", walletDto.Id, expiration);
    }

    private async Task<WalletDto?> GetWalletCacheByIdAsync(Guid id)
    {
        var key = CacheKeys.GetWalletByIdKey(id);
        return await cacheService.GetAsync<WalletDto>(key);
    }

    private async Task<WalletDto?> GetWalletCacheByCodeAsync(string code)
    {
        var key = CacheKeys.GetWalletByCodeKey(code);
        return await cacheService.GetAsync<WalletDto>(key);
    }

    private async Task InvalidateWalletCacheAsync(Core.Model.Wallet wallet)
    {
        await cacheService.RemoveAsync(CacheKeys.GetWalletByIdKey(wallet.Id));
        await cacheService.RemoveAsync(CacheKeys.GetWalletByCodeKey(wallet.Code));

        logger.LogDebug("Cache invalidated for wallet {WalletId} and code {WalletCode}", wallet.Id, wallet.Code);
    }
}
