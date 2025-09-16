using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Interface;
using Wallet.Core.Interface;

namespace Wallet.Application.Consumer;

public class UserCreatedConsumer(
    IServiceProvider serviceProvider,
    ILogger<UserCreatedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageConsumer = serviceProvider.GetRequiredService<IMessageConsumer>();

        const string exchange = "user-exchange";
        const string queue = "wallet-user-created";
        const string routingKey = "user.created";

        logger.LogInformation("Wallet service listening to '{Queue}' on exchange '{Exchange}' with routing key '{RoutingKey}'",
            queue, exchange, routingKey);

        await messageConsumer.ConsumeAsync(
            exchange: exchange,
            queue: queue,
            routingKey: routingKey,
            onMessageReceived: async (message) =>
            {
                try
                {
                    var userDto = JsonSerializer.Deserialize<UserDto>(message);
                    if (userDto is null)
                    {
                        logger.LogWarning("Received invalid or null UserDto from message: {Message}", message);
                        return;
                    }

                    using var scope = serviceProvider.CreateScope();
                    var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();
                    await walletService.CreateAsync(userDto.Id);
                    logger.LogInformation("Wallet created for user ID {UserId}", userDto.Id);
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Failed to deserialize UserDto from message: {Message}", message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process 'user.created' event");
                }
            });
    }
}
