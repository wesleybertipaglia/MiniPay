using Microsoft.Extensions.Logging;
using Shared.Core.Extensions;
using Shared.Core.Helpers;
using Shared.Core.Interface;
using Verification.Core.Dto;
using Verification.Core.Enum;
using Verification.Core.Extension;
using Verification.Core.Interface;
using Verification.Core.Mapper;
using Verification.Core.Model;

namespace Verification.Application.Service;

public class VerificationCodeService(
    IVerificationCodeRepository repository,
    ICacheService cacheService,
    ILogger<VerificationCodeService> logger
) : IVerificationCodeService
{
    public async Task<VerificationCodeDto> CreateAsync(Guid userId, VerificationCodeType type)
    {
        var codeCacheKey = CacheKeysHelper.GetVerificationCodeKeyByUserIdAndType(userId, type.ToString());
        var cachedCode = await cacheService.GetAsync<VerificationCodeDto>(codeCacheKey);

        if (cachedCode != null && cachedCode.IsValid())
        {
            return cachedCode;
        }
        
        var existing = await repository.GetLatestValidCodeByUserIdAndTypeAsync(userId, type);

        if (existing != null && existing.IsValid())
        {
            return existing.Map();
        }

        var entity = new VerificationCode(userId, type);
        var created = await repository.CreateAsync(entity);

        await cacheService.SetAsync(codeCacheKey, created.Map(), created.ExpiresAt.ToExpirationTimeSpan());

        LogHelper.LogInfo(logger, $"Generated new verification code for user {userId} of type {type}");

        return created.Map();
    }

    public async Task ValidateAsync(Guid userId, string code)
    {
        var existing = await repository.GetByUserIdAndCodeAsync(userId, code);

        if (existing == null || !existing.IsValid())
        {
            LogHelper.LogError(logger, $"No valid verification code found for user {userId}.");
            throw new InvalidOperationException("Verification code not found or expired.");
        }

        if (!existing.Matches(code))
        {
            LogHelper.LogError(logger, $"Verification code mismatch for user {userId}.");
            throw new InvalidOperationException("Verification code mismatch.");
        }

        existing.MarkAsUsed();
        await repository.UpdateAsync(existing);

        var codeCacheKey = CacheKeysHelper.GetVerificationCodeKeyByUserIdAndType(userId, existing.Type.ToString());
        await cacheService.RemoveAsync(codeCacheKey);

        LogHelper.LogInfo(logger, $"Marked verification code as used for user {userId}");
    }
}
