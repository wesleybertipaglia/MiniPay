using Notification.Core.Dto;
using Notification.Core.Interface;

namespace Notification.Infrastructure.Service;

public class ConsoleEmailService: IEmailService
{
    public Task SendEmailAsync(EmailRequestDto emailRequestDto)
    {
        Console.WriteLine($"Sending e-mail to: {emailRequestDto.To}");
        Console.WriteLine($"Subject: {emailRequestDto.Subject}");
        Console.WriteLine($"Body: {emailRequestDto.Body}");
        return Task.CompletedTask;
    }
}