namespace Notification.Core.Dto;

public record EmailRequestDto
(
    string To,
    string Subject,
    string Body
);