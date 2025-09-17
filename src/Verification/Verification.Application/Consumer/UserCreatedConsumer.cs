using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Shared.Core.Dto;
using Shared.Core.Interface;
using Verification.Core.Enum;
using Verification.Core.Interface;

namespace Verification.Application.Consumer;

public class UserCreatedConsumer(
    IServiceProvider serviceProvider,
    ILogger<UserCreatedConsumer> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageConsumer = serviceProvider.GetRequiredService<IMessageConsumer>();
        
        const string exchange = "user-exchange";
        const string queue = "new-verification";
        const string routingKey = "user.created";

        logger.LogInformation("Starting consumer for queue '{Queue}' on exchange '{Exchange}' with routing key '{RoutingKey}'",
            queue, exchange, routingKey);

        await messageConsumer.ConsumeAsync(
            exchange: exchange,
            queue: queue,
            routingKey: routingKey,
            async message =>
            {
                try
                {
                    var userDto = JsonSerializer.Deserialize<UserDto>(message);

                    if (userDto is null)
                    {
                        logger.LogWarning("Received invalid or null UserDto in user.created event.");
                        return;
                    }

                    using var scope = serviceProvider.CreateScope();
                    var verificationService = scope.ServiceProvider.GetRequiredService<IVerificationCodeService>();

                    await verificationService.CreateAsync(userDto.Id, VerificationCodeType.EMAIL_VERIFICATION);

                    logger.LogInformation("Verification code generated for user {UserId}", userDto.Id);
                }
                catch (JsonException jsonEx)
                {
                    logger.LogError(jsonEx, "Failed to deserialize user.created message: {Message}", message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing user.created event");
                }
            });
    }
}