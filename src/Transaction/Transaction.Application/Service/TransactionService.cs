using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Enum;
using Shared.Core.Interface;
using Transaction.Core.Dto;
using Transaction.Core.Helper;
using Transaction.Core.Interface;
using Transaction.Core.Mapper;
using Transaction.Core.Validator;

namespace Transaction.Application.Service;

public class TransactionService(
    ITransactionRepository transactionRepository,
    ILogger<TransactionService> logger,
    ICacheService cacheService,
    IMessagePublisher messagePublisher)
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

    public async Task<TransactionDto> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentNullException(nameof(code));

        logger.LogDebug("Fetching transaction by Code: {TransactionCode}", code);

        var cached = await GetTransactionCacheByCodeAsync(code);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for transaction: {TransactionCode}", code);
            return cached;
        }

        var transaction = await transactionRepository.GetByCodeAsync(code).ConfigureAwait(false);
        if (transaction is null)
        {
            logger.LogWarning("Transaction not found for Code: {TransactionCode}", code);
            throw new KeyNotFoundException($"Transaction with code {code} not found.");
        }

        var dto = transaction.ToDetailsDto();
        await SetTransactionCacheAsync(dto);

        logger.LogInformation("Transaction retrieved from DB and cached: {TransactionCode}", code);

        return dto;
    }

    public async Task<TransactionDto> CreateAsync(UserDto userDto, TransactionRequestDto requestDto)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        
        if (userDto.Id == Guid.Empty)
            throw new ArgumentNullException(nameof(userDto.Id));

        logger.LogDebug("Creating new transaction for user {UserId}", userDto.Id);
        
        TransactionValidator.Validate(
            type: requestDto.Type,
            targetWalletCode: requestDto.TargetWalletCode,
            targetTransactionCode: requestDto.TargetTransactionCode
        );
        
        var transaction = requestDto.ToEntity(userDto.Id);

        await transactionRepository.CreateAsync(transaction).ConfigureAwait(false);
        await RemoveTransactionCacheAsync(transaction).ConfigureAwait(false);

        var transactionDto = transaction.ToDetailsDto();
        var transactionEventDto = new TransactionEventDto(userDto,  transactionDto, false);
        var messageJson = JsonSerializer.Serialize(transactionEventDto);
        
        await messagePublisher.PublishAsync(
            exchange: "transaction-exchange",
            routingKey: "transaction.created",
            message: messageJson);

        logger.LogInformation("Transaction created and event published: {TransactionId}", transaction.Id);

        return transactionDto;
    }

    
    public async Task<TransactionDto> UpdateStatusAsync(UserDto userDto, TransactionDto transactionDto, bool success)
    {
        ArgumentNullException.ThrowIfNull(userDto);
        ArgumentNullException.ThrowIfNull(transactionDto);

        logger.LogDebug("Updating transaction status. Code: {TransactionCode}, UserId: {UserId}", transactionDto.Code, userDto.Id);

        var transaction = await transactionRepository.GetByCodeAsync(transactionDto.Code).ConfigureAwait(false);
        if (transaction is null)
        {
            logger.LogWarning("Transaction not found for update. Code: {TransactionCode}", transactionDto.Code);
            throw new KeyNotFoundException($"Transaction with code {transactionDto.Code} not found.");
        }

        if (transaction.UserId != userDto.Id)
        {
            logger.LogWarning("User {UserId} unauthorized to update transaction {TransactionCode}", userDto.Id, transactionDto.Code);
            throw new UnauthorizedAccessException("You do not have permission to update this transaction.");
        }

        TransactionValidator.Validate(
            type: transaction.Type,
            targetWalletCode: transaction.TargetWalletCode,
            targetTransactionCode: transaction.TargetTransactionCode
        );

        transaction.UpdateStatus(success);

        var updated = await transactionRepository.UpdateAsync(transaction).ConfigureAwait(false);
        await RemoveTransactionCacheAsync(updated).ConfigureAwait(false);

        var updatedDto = updated.ToDetailsDto();

        logger.LogInformation("Transaction updated and event published. ID: {TransactionId}", updated.Id);

        return updatedDto;
    }
    
    private async Task<TransactionDto?> GetTransactionCacheByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentNullException(nameof(code));

        var cacheKey = TransactionCacheKeys.GetTransactionByCodeKey(code);

        var cached = await cacheService.GetAsync<TransactionDto>(cacheKey).ConfigureAwait(false);

        if (cached is not null)
        {
            logger.LogDebug("Transaction cache hit. Code: {TransactionCode}", code);
            return cached;
        }

        logger.LogDebug("Transaction cache miss. Code: {TransactionCode}", code);
        return null;
    }
    
    private async Task SetTransactionCacheAsync(TransactionDto  transactionDto)
    {
        var codeKey = TransactionCacheKeys.GetTransactionByCodeKey(transactionDto.Code);
        await cacheService.SetAsync(codeKey, transactionDto, TimeSpan.FromMinutes(5)).ConfigureAwait(false);
        logger.LogDebug("Transaction cached. Code: {TransactionCode}", transactionDto.Code);
    }

    private async Task RemoveTransactionCacheAsync(Core.Model.Transaction transaction)
    {
        var codeKey = TransactionCacheKeys.GetTransactionByCodeKey(transaction.Code);
        await cacheService.RemoveAsync(codeKey).ConfigureAwait(false);
        logger.LogDebug("Transaction cache invalidated. ID: {TransactionId}", transaction.Id);
    }
}
