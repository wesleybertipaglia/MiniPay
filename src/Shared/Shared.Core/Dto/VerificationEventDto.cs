namespace Shared.Core.Dto;

public record VerificationEventDto
(
    UserDto User,
    string Code
);