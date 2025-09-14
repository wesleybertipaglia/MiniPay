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

        await messageConsumer.ConsumeAsync(
            exchange: "user-exchange",
            queue: "verification user created",
            routingKey: "user.created",
            async message =>
            {
                try
                {
                    var userDto = JsonSerializer.Deserialize<UserDto>(message);
                    if (userDto == null)
                    {
                        logger.LogError("Invalid user DTO.");
                        return;
                    }
                    
                    using var scope = serviceProvider.CreateScope();
                    var verificationService = scope.ServiceProvider.GetRequiredService<IVerificationCodeService>();
                    await verificationService.CreateAsync(userDto.Id, VerificationCodeType.EMAIL_VERIFICATION);

                    logger.LogInformation($"Verification code created for user {userDto.Id}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing message");
                }
            });
    }
}
