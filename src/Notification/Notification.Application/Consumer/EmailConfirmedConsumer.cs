using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Core.Interface;
using Notification.Core.Template;
using Shared.Core.Dto;
using Shared.Core.Interface;

namespace Notification.Application.Consumer;

public class EmailConfirmedConsumer(
    IServiceProvider serviceProvider,
    ILogger<EmailConfirmedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageConsumer = serviceProvider.GetRequiredService<IMessageConsumer>();

        await messageConsumer.ConsumeAsync(
            exchange: "user-exchange",
            queue: "email-confirmed-notification",
            routingKey: "email.confirmed",
            onMessageReceived: async (message) =>
            {
                try
                {
                    var userDto = JsonSerializer.Deserialize<UserDto>(message);
                    if (userDto is null)
                    {
                        logger.LogError("Received null or invalid UserDto.");
                        return;
                    }

                    using var scope = serviceProvider.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var emailRequest = EmailTemplate.BuildWelcomeEmail(userDto);

                    await emailService.SendEmailAsync(emailRequest);

                    logger.LogInformation("Confirmation email sent to user {UserId} ({Email})", userDto.Id, userDto.Email);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process email.confirmed event for notification");
                }
            });
    }
}