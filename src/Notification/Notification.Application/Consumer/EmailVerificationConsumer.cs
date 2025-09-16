using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Core.Interface;
using Notification.Core.Template;
using Shared.Core.Dto;
using Shared.Core.Interface;

namespace Notification.Application.Consumer;

public class EmailVerificationConsumer(
    IServiceProvider serviceProvider,
    ILogger<EmailVerificationConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageConsumer = serviceProvider.GetRequiredService<IMessageConsumer>();

        await messageConsumer.ConsumeAsync(
            exchange: "user-exchange",
            queue: "new-user-notification",
            routingKey: "email.verification",
            onMessageReceived: async (message) =>
            {
                using var scope = serviceProvider.CreateScope();

                try
                {
                    var emailVerificationEventDto = JsonSerializer.Deserialize<VerificationCodeNotificationDto>(message);

                    if (emailVerificationEventDto is null)
                    {
                        logger.LogWarning("Received null or invalid EmailVerificationEventDto: {RawMessage}", message);
                        return;
                    }

                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var emailRequest = EmailTemplate.BuildConfirmEmail(emailVerificationEventDto.User, emailVerificationEventDto.Code);

                    await emailService.SendEmailAsync(emailRequest);

                    logger.LogInformation(
                        "Verification email sent to user {UserId} ({Email}) with code {Code}",
                        emailVerificationEventDto.User.Id,
                        emailVerificationEventDto.User.Email,
                        emailVerificationEventDto.Code
                    );
                }
                catch (JsonException jsonEx)
                {
                    logger.LogError(jsonEx, "Failed to deserialize message in email.verification: {Message}", message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process email.verification event");
                }
            });
    }
}
