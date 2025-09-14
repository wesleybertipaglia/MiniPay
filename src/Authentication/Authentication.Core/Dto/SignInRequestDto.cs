namespace Authentication.Core.Dto;

public record SignInRequestDto
(
    string Email,
    string Password
);