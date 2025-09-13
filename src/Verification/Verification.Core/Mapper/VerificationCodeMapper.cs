using Verification.Core.Dto;
using Verification.Core.Model;

namespace Verification.Core.Mapper;

public static class VerificationCodeMapper
{
    public static VerificationCode ToEntity(this VerificationCodeDtos.CreateDto dto)
    {
        return dto == null ? throw new ArgumentNullException(nameof(dto)) : new VerificationCode(dto.UserId);
    }

    public static VerificationCodeDtos.SummaryDto ToSummaryDto(this VerificationCode entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        return new VerificationCodeDtos.SummaryDto(
            Content: entity.Content,
            Used: entity.Used
        );
    }

    public static VerificationCodeDtos.DetailsDto ToDetailsDto(this VerificationCode entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        return new VerificationCodeDtos.DetailsDto(
            Content: entity.Content,
            Used: entity.Used,
            VerifiedAt: entity.VerifiedAt,
            ExpiresAt: entity.ExpiresAt
        );
    }
}