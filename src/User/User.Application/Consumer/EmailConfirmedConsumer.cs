using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Core.Interface;
using User.Core.Interface;

namespace User.Application.Consumer;

public class EmailConfirmedConsumer(
    IServiceProvider serviceProvider,
    ILogger<EmailConfirmedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageConsumer = serviceProvider.GetRequiredService<IMessageConsumer>();

        const string exchange = "user-exchange";
        const string queue = "email-confirmed";
        const string routingKey = "email.confirmed";

        logger.LogInformation("Starting consumer for queue '{Queue}' on exchange '{Exchange}' with routing key '{RoutingKey}'",
            queue, exchange, routingKey);

        await messageConsumer.ConsumeAsync(
            exchange: exchange,
            queue: queue,
            routingKey: routingKey,
            onMessageReceived: async (message) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(message);
                    if (!doc.RootElement.TryGetProperty("userId", out var userIdProperty))
                    {
                        logger.LogWarning("Message does not contain 'userId' property: {Message}", message);
                        return;
                    }

                    var userId = userIdProperty.GetGuid();

                    using var scope = serviceProvider.CreateScope();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                    await userService.ConfirmEmailAsync(userId);

                    logger.LogInformation("Successfully processed 'email.confirmed' event for user ID {UserId}", userId);
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Failed to deserialize message: {Message}", message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process 'email.confirmed' event");
                }
            });
    }
}
