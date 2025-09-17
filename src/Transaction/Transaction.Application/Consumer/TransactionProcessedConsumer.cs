using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Interface;
using Transaction.Core.Interface;

namespace Transaction.Application.Consumer;

public class TransactionProcessedConsumer(
    IServiceProvider serviceProvider,
    ILogger<TransactionProcessedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageConsumer = serviceProvider.GetRequiredService<IMessageConsumer>();

        const string exchange = "transaction-exchange";
        const string queue = "transaction-processed";
        const string routingKey = "transaction.processed";

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
                    var transactionEventDto = JsonSerializer.Deserialize<TransactionEventDto>(message);
                    if (transactionEventDto is null)
                    {
                        logger.LogWarning("Invalid or null TransactionProcessedEvent received: {Message}", message);
                        return;
                    }

                    using var scope = serviceProvider.CreateScope();
                    var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

                    var (userDto, transactionDto, success) = transactionEventDto;
                    var updatedTransaction = await transactionService.UpdateStatusAsync(userDto, transactionDto, success);

                    logger.LogInformation("Transaction {Code} status updated to {Status}", updatedTransaction.Code, updatedTransaction.Status);
                    
                    var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
                    var messageJson = JsonSerializer.Serialize(updatedTransaction);

                    await messagePublisher.PublishAsync(
                        exchange: exchange,
                        routingKey: "transaction.notification",
                        message: messageJson);

                    logger.LogInformation("Notification event published for transaction {Code}", updatedTransaction.Code);
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Failed to deserialize TransactionProcessedEvent: {Message}", message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error processing transaction.processed event");
                }
            });
    }
}
