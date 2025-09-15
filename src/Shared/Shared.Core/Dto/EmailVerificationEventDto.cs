namespace Shared.Core.Dto;

public record EmailVerificationEventDto
(
    UserDto User,
    string Code
);