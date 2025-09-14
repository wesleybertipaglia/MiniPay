namespace Authentication.Core.Dto;

public record SignUpRequestDto
(
    string Name,
    string Email,
    string Password
);
