namespace Authentication.Core.Dto;

public record TokenDto
(
    string Content,
    DateTime Expires
);