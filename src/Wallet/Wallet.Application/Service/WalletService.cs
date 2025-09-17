using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Enum;
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

        var mappedWallet = wallet.ToDto();
        await CacheWalletAsync(mappedWallet);

        logger.LogInformation("Wallet {WalletId} fetched from DB and cached", wallet.Id);
        return mappedWallet;
    }

    public async Task<WalletDto> CreateAsync(Guid userId)
    {
        var wallet = new Core.Model.Wallet(userId);
        await walletRepository.CreateAsync(wallet);
        await RemoveWalletCacheAsync(wallet);

        logger.LogInformation("Wallet {WalletId} created", wallet.Id);
        return wallet.ToDto();
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
        await RemoveWalletCacheAsync(updatedWallet);

        logger.LogInformation("Wallet {WalletId} updated", updatedWallet.Id);
        return updatedWallet.ToDto();
    }
    
    public async Task<bool> UpdateBalanceAsync(TransactionDto transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        logger.LogDebug("Processing transaction: {Code}, Type: {Type}, Amount: {Amount}, UserId: {UserId}",
            transaction.Code, transaction.Type, transaction.Amount, transaction.UserId);

        var wallet = await walletRepository.GetByUserIdAsync(transaction.UserId);
        if (wallet is null) return false;
        
        await RemoveWalletCacheAsync(wallet);

        try
        {
            return transaction.Type switch
            {
                TransactionType.DEPOSIT => await ProcessDepositAsync(wallet, transaction),
                TransactionType.WITHDRAW => await ProcessWithdrawAsync(wallet, transaction),
                TransactionType.TRANSFER => await ProcessTransferAsync(wallet, transaction),
                TransactionType.PAYMENT => await ProcessPaymentAsync(wallet, transaction),
                TransactionType.REFUND => await ProcessRefundAsync(wallet, transaction),
                _ => LogAndFail($"Unsupported transaction type: {transaction.Type}")
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing transaction {Code}", transaction.Code);
            return false;
        }
    }
    
    private async Task<bool> ProcessDepositAsync(Core.Model.Wallet wallet, TransactionDto transaction)
    {
        wallet.Deposit(transaction.Amount);
        await walletRepository.UpdateAsync(wallet);
        logger.LogInformation("Deposit successful. WalletId: {WalletId}, NewBalance: {Balance}", wallet.Id, wallet.Balance);
        return true;
    }

    private async Task<bool> ProcessWithdrawAsync(Core.Model.Wallet wallet, TransactionDto transaction)
    {
        wallet.Withdraw(transaction.Amount);
        await walletRepository.UpdateAsync(wallet);
        logger.LogInformation("Withdraw successful. WalletId: {WalletId}, NewBalance: {Balance}", wallet.Id, wallet.Balance);
        return true;
    }

    private async Task<bool> ProcessTransferAsync(Core.Model.Wallet wallet, TransactionDto transaction)
    {
        return await TransferBetweenWalletsAsync(wallet, transaction, logContext: "Transfer");
    }

    private async Task<bool> ProcessPaymentAsync(Core.Model.Wallet wallet, TransactionDto transaction)
    {
        return await TransferBetweenWalletsAsync(wallet, transaction, logContext: "Payment");
    }

    private async Task<bool> ProcessRefundAsync(Core.Model.Wallet wallet, TransactionDto transaction)
    {
        if (!string.IsNullOrWhiteSpace(transaction.TargetTransactionCode))
            return await TransferBetweenWalletsAsync(wallet, transaction, logContext: "Refund");
        logger.LogWarning("Refund requires TargetTransactionCode. Code: {Code}", transaction.Code);
        return false;
    }
    
    private async Task<bool> TransferBetweenWalletsAsync(Core.Model.Wallet wallet, TransactionDto transaction, string logContext)
    {
        if (string.IsNullOrWhiteSpace(transaction.TargetWalletCode))
        {
            logger.LogWarning("{Context}: Target wallet code is missing. Transaction: {Code}", logContext, transaction.Code);
            return false;
        }

        var targetWallet = await walletRepository.GetByCodeAsync(transaction.TargetWalletCode!);
        if (targetWallet is null)
        {
            logger.LogWarning("{Context}: Target wallet not found. Code: {Code}, TargetWalletCode: {TargetWalletCode}",
                logContext, transaction.Code, transaction.TargetWalletCode);
            return false;
        }

        wallet.Withdraw(transaction.Amount);
        targetWallet.Deposit(transaction.Amount);
        await walletRepository.UpdateAsync(wallet);
        await walletRepository.UpdateAsync(targetWallet);
        await RemoveWalletCacheAsync(targetWallet);

        logger.LogInformation("{Context} successful. OriginWalletId: {OriginWalletId}, TargetWalletId: {TargetWalletId}",
            logContext, wallet.Id, targetWallet.Id);

        return true;
    }
    
    private bool LogAndFail(string message)
    {
        logger.LogWarning(message);
        return false;
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

    private async Task RemoveWalletCacheAsync(Core.Model.Wallet wallet)
    {
        await cacheService.RemoveAsync(WalletCacheKeys.GetWalletByUserIdKey(wallet.UserId));

        logger.LogDebug("Cache invalidated for wallet {WalletId} and code {WalletCode}", wallet.Id, wallet.Code);
    }
}
