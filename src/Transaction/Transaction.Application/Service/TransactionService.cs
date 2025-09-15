using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Enum;
using Shared.Core.Interface;
using Transaction.Core.Dto;
using Transaction.Core.Helper;
using Transaction.Core.Interface;
using Transaction.Core.Mapper;

namespace Transaction.Application.Service;

public class TransactionService(
    ITransactionRepository transactionRepository,
    ILogger<TransactionService> logger,
    ICacheService cacheService)
    : ITransactionService
{
    public async Task<IEnumerable<TransactionSummaryDto>> ListAsync(
        Guid userId,
        int page = 1,
        int size = 10,
        TransactionType? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (userId == Guid.Empty) 
            throw new ArgumentNullException(nameof(userId));

        logger.LogDebug("Listing transactions for user {UserId} - Page: {Page}, Size: {Size}, Type: {Type}, StartDate: {StartDate}, EndDate: {EndDate}",
            userId, page, size, type, startDate, endDate);

        var transactions = await transactionRepository.ListAsync(
            userId: userId,
            type: type,
            page: page,
            size: size,
            startDate: startDate,
            endDate: endDate).ConfigureAwait(false);

        var list = transactions.Select(t => t.ToSummaryDto()).ToList();

        logger.LogInformation("Retrieved {Count} transactions for user {UserId}", list.Count, userId);

        return list;
    }

    public async Task<TransactionDetailsDto> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));

        logger.LogDebug("Fetching transaction by ID: {TransactionId}", id);

        var cacheKey = TransactionCacheKeys.GetTransactionByIdKey(id);
        var cached = await cacheService.GetAsync<TransactionDetailsDto>(cacheKey).ConfigureAwait(false);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for transaction: {TransactionId}", id);
            return cached;
        }

        var transaction = await transactionRepository.GetByIdAsync(id).ConfigureAwait(false);
        if (transaction is null)
        {
            logger.LogWarning("Transaction not found for ID: {TransactionId}", id);
            throw new KeyNotFoundException($"Transaction with ID {id} not found.");
        }

        var dto = transaction.ToDetailsDto();
        await cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5)).ConfigureAwait(false);

        logger.LogInformation("Transaction retrieved from DB and cached: {TransactionId}", id);

        return dto;
    }

    public async Task<TransactionDetailsDto> CreateAsync(TransactionRequestDto request, Guid userId)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));

        logger.LogDebug("Creating new transaction for user {UserId}", userId);

        var transaction = request.ToEntity(userId);

        await transactionRepository.CreateAsync(transaction).ConfigureAwait(false);
        await InvalidateTransactionCacheAsync(transaction).ConfigureAwait(false);

        logger.LogInformation("Transaction successfully created: {TransactionId}", transaction.Id);

        return transaction.ToDetailsDto();
    }

    public async Task<TransactionDetailsDto> UpdateStatusAsync(TransactionDto transactionDto, Guid userId)
    {
        if (transactionDto is null)
            throw new ArgumentNullException(nameof(transactionDto));
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));

        logger.LogDebug("Updating transaction status. Code: {TransactionCode}, UserId: {UserId}", transactionDto.Code, userId);

        var transaction = await transactionRepository.GetByCodeAsync(transactionDto.Code).ConfigureAwait(false);
        if (transaction is null)
        {
            logger.LogWarning("Transaction not found for update. Code: {TransactionCode}", transactionDto.Code);
            throw new KeyNotFoundException($"Transaction with code {transactionDto.Code} not found.");
        }

        if (transaction.UserId != userId)
        {
            logger.LogWarning("User {UserId} unauthorized to update transaction {TransactionCode}", userId, transactionDto.Code);
            throw new UnauthorizedAccessException("You do not have permission to update this transaction.");
        }

        transaction.UpdateStatus(transactionDto.Status);

        var updated = await transactionRepository.UpdateAsync(transaction).ConfigureAwait(false);
        await InvalidateTransactionCacheAsync(updated).ConfigureAwait(false);

        logger.LogInformation("Transaction updated successfully. ID: {TransactionId}", updated.Id);

        return updated.ToDetailsDto();
    }

    private async Task InvalidateTransactionCacheAsync(Core.Model.Transaction transaction)
    {
        var idKey = TransactionCacheKeys.GetTransactionByIdKey(transaction.Id);
        var codeKey = TransactionCacheKeys.GetTransactionByCodeKey(transaction.Code);

        await cacheService.RemoveAsync(idKey).ConfigureAwait(false);
        await cacheService.RemoveAsync(codeKey).ConfigureAwait(false);

        logger.LogDebug("Transaction cache invalidated. ID: {TransactionId}", transaction.Id);
    }
}
