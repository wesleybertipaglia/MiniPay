using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Extension;
using Shared.Core.Interface;
using Verification.Core.Dto;
using Verification.Core.Enum;
using Verification.Core.Extension;
using Verification.Core.Helper;
using Verification.Core.Interface;
using Verification.Core.Mapper;
using Verification.Core.Model;

namespace Verification.Application.Service;

public class VerificationCodeService(
    IVerificationCodeRepository repository,
    ICacheService cacheService,
    ILogger<VerificationCodeService> logger,
    IMessagePublisher messagePublisher
) : IVerificationCodeService
{
    public async Task<VerificationCodeDto> CreateAsync(Guid userId, VerificationCodeType type)
    {
        var codeCacheKey = CacheKeys.GetVerificationCodeKeyByUserIdAndType(userId, type.ToString());
        var cachedCode = await cacheService.GetAsync<VerificationCodeDto>(codeCacheKey);

        if (cachedCode is not null && cachedCode.IsValid())
        {
            logger.LogInformation("Using valid cached verification code for user {UserId} and type {Type}", userId, type);
            return cachedCode;
        }

        var existing = await repository.GetLatestValidCodeByUserIdAndTypeAsync(userId, type);
        if (existing is not null && existing.IsValid())
        {
            logger.LogInformation("Using existing valid verification code from DB for user {UserId} and type {Type}", userId, type);
            return existing.Map();
        }

        var entity = new VerificationCode(userId, type);
        var created = await repository.CreateAsync(entity);

        await cacheService.SetAsync(codeCacheKey, created.Map(), created.ExpiresAt.ToExpirationTimeSpan());
        logger.LogInformation("Created new verification code for user {UserId} of type {Type}", userId, type);

        var userCacheKey = CacheKeys.GetUserByIdKey(userId);
        var cachedUser = await cacheService.GetAsync<UserDto>(userCacheKey);

        if (cachedUser is null)
        {
            logger.LogWarning("User cache not found for user {UserId}. Skipping email.verification publish.", userId);
        }
        else
        {
            var emailVerificationEvent = new VerificationEventDto(cachedUser, created.Content);
            var messageJson = JsonSerializer.Serialize(emailVerificationEvent);

            await messagePublisher.PublishAsync(
                exchange: "user-exchange",
                routingKey: "email.verification",
                message: messageJson
            );

            logger.LogInformation("Published email.verification event for user {UserId}", userId);
        }

        return created.Map();
    }

    public async Task ValidateAsync(Guid userId, string code)
    {
        var existing = await repository.GetByUserIdAndCodeAsync(userId, code);

        if (existing is null || !existing.IsValid())
        {
            logger.LogWarning("Invalid or expired verification code for user {UserId}", userId);
            throw new InvalidOperationException("Verification code not found or expired.");
        }

        if (!existing.Matches(code))
        {
            logger.LogWarning("Verification code mismatch for user {UserId}", userId);
            throw new InvalidOperationException("Verification code mismatch.");
        }

        existing.MarkAsUsed();
        await repository.UpdateAsync(existing);

        var codeCacheKey = CacheKeys.GetVerificationCodeKeyByUserIdAndType(userId, existing.Type.ToString());
        await cacheService.RemoveAsync(codeCacheKey);

        logger.LogInformation("Verification code marked as used for user {UserId}", userId);

        var userCacheKey = CacheKeys.GetUserByIdKey(userId);
        var cachedUser = await cacheService.GetAsync<UserDto>(userCacheKey);

        if (cachedUser is null)
        {
            logger.LogWarning("User cache not found for user {UserId}. Skipping email.confirmed publish.", userId);
            return;
        }

        var messageJson = JsonSerializer.Serialize(cachedUser);

        await messagePublisher.PublishAsync(
            exchange: "user-exchange",
            routingKey: "email.confirmed",
            message: messageJson
        );

        logger.LogInformation("Published email.confirmed event for user {UserId}", userId);
    }
}
