using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Core.Dto;
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
                try
                {
                    var emailVerificationEventDto = JsonSerializer.Deserialize<EmailVerificationEventDto>(message);
                    if (emailVerificationEventDto is null)
                    {
                        logger.LogError("Received null or invalid emailVerificationEventDto.");
                        return;
                    }

                    using var scope = serviceProvider.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var emailRequest = EmailTemplate.BuildConfirmationEmail(emailVerificationEventDto.User, emailVerificationEventDto.Code);

                    await emailService.SendEmailAsync(emailRequest);

                    logger.LogInformation("Welcome email sent to user {UserId} ({Email})", emailVerificationEventDto.User.Id, emailVerificationEventDto.User.Email);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process user.created event for notification");
                }
            });
    }
}
