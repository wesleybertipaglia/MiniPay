using Notification.Core.Dto;

namespace Notification.Core.Interface;

public interface IEmailService
{
    Task SendEmailAsync(EmailRequestDto emailRequestDto);
}