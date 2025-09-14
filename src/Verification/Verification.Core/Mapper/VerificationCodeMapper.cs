using Verification.Core.Dto;
using Verification.Core.Model;

namespace Verification.Core.Mapper;

public static class VerificationCodeMapper
{
    public static VerificationCodeDto Map(this VerificationCode entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        return new VerificationCodeDto(
            UserId: entity.UserId,
            Content: entity.Content,
            Used: entity.Used,
            VerifiedAt: entity.VerifiedAt,
            ExpiresAt: entity.ExpiresAt
        );
    }
}