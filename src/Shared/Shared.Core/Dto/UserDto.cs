namespace Shared.Core.Dto;

public record UserDto
(
    Guid Id,
    string Code,
    string Name,
    string Email,
    bool EmailConfirmed
);