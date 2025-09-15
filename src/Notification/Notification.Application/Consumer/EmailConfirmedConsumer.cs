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
                using var scope = serviceProvider.CreateScope();

                try
                {
                    var userDto = JsonSerializer.Deserialize<UserDto>(message);

                    if (userDto is null)
                    {
                        logger.LogWarning("Received null or invalid UserDto in email.confirmed message: {RawMessage}", message);
                        return;
                    }

                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var emailRequest = EmailTemplate.BuildWelcomeEmail(userDto);

                    await emailService.SendEmailAsync(emailRequest);

                    logger.LogInformation("Welcome email sent to user {UserId} ({Email})", userDto.Id, userDto.Email);
                }
                catch (JsonException jsonEx)
                {
                    logger.LogError(jsonEx, "Failed to deserialize message in email.confirmed: {Message}", message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled error while processing email.confirmed event for user");
                }
            });
    }
}
