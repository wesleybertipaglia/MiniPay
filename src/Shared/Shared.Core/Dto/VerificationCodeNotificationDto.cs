namespace Shared.Core.Dto;

public record VerificationCodeNotificationDto
(
    UserDto User,
    string Code
);