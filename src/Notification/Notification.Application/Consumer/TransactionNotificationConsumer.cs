using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Core.Interface;
using Notification.Core.Template;
using Shared.Core.Dto;
using Shared.Core.Interface;

namespace Notification.Application.Consumer;

public class TransactionNotificationConsumer(
    IServiceProvider serviceProvider,
    ILogger<TransactionNotificationConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageConsumer = serviceProvider.GetRequiredService<IMessageConsumer>();

        const string exchange = "transaction-exchange";
        const string queue = "transaction-notification";
        const string routingKey = "transaction.notification";

        logger.LogInformation("Starting consumer for queue '{Queue}' on exchange '{Exchange}' with routing key '{RoutingKey}'",
            queue, exchange, routingKey);

        await messageConsumer.ConsumeAsync(
            exchange: exchange,
            queue: queue,
            routingKey: routingKey,
            onMessageReceived: async (message) =>
            {
                using var scope = serviceProvider.CreateScope();

                try
                {
                    var transactionEventDto = JsonSerializer.Deserialize<TransactionEventDto>(message);

                    if (transactionEventDto is null)
                    {
                        logger.LogWarning("Received null or invalid TransactionDto in transaction.notification message: {RawMessage}", message);
                        return;
                    }
                    
                    var (userDto, transactionDto, success) = transactionEventDto;
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var emailRequest = EmailTemplate.BuildTransactionEmail(userDto, transactionDto, success);

                    await emailService.SendEmailAsync(emailRequest);

                    logger.LogInformation("Transaction notification email sent. Code: {Code}, UserId: {UserId}, Success: {Success}",
                        transactionDto.Code, userDto.Id, success);
                }
                catch (JsonException jsonEx)
                {
                    logger.LogError(jsonEx, "Failed to deserialize message in transaction.notification: {Message}", message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled error while processing transaction.notification event");
                }
            });
    }
}
