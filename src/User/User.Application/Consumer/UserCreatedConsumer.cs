using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Interface;
using User.Core.Interface;

namespace User.Application.Consumer;

public class UserCreatedConsumer(
    IServiceProvider serviceProvider,
    ILogger<UserCreatedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        await messageConsumer.ConsumeAsync(
            exchange: "user-exchange",
            queue: "new user",
            routingKey: "user.created",
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

                    await userService.CreateAsync(userDto);
                    logger.LogInformation("Processed user.created event for user {UserId}", userDto.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process user.created event");
                }
            });
    }
}