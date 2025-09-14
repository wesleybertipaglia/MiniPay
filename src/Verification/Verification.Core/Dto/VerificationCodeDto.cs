namespace Verification.Core.Dto;

public record VerificationCodeDto(
    Guid UserId,
    string Content,
    bool Used,
    DateTime? VerifiedAt,
    DateTime ExpiresAt
);