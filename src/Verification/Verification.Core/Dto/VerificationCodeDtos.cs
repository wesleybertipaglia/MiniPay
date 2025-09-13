namespace Verification.Core.Dto;

public class VerificationCodeDtos
{
    public record CreateDto(
        Guid UserId
    );
    
    public record SummaryDto(
        string Content,
        bool Used
    );

    public record DetailsDto(
        string Content,
        bool Used,
        DateTime? VerifiedAt,
        DateTime ExpiresAt
    );
}