using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Core.Dto;
using Shared.Core.Enum;
using Shared.Core.Interface;
using Wallet.Core.Interface;

namespace Wallet.Application.Consumer;

public class TransactionCreatedConsumer(
    IServiceProvider serviceProvider,
    ILogger<TransactionCreatedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageConsumer = serviceProvider.GetRequiredService<IMessageConsumer>();
        var messagePublisher = serviceProvider.GetRequiredService<IMessagePublisher>();

        const string exchange = "transaction-exchange";
        const string queue = "wallet-updated";
        const string routingKey = "transaction.created";

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
                        logger.LogWarning("Invalid or null TransactionDto received: {Message}", message);
                        return;
                    }

                    var (userDto, transactionDto, _) = transactionEventDto;
                    
                    using var scope = serviceProvider.CreateScope();
                    var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();

                    var success = await walletService.UpdateBalanceAsync(transactionEventDto.TransactionDto);
                    var transactionProcessedEvent = new TransactionEventDto(userDto, transactionDto, success);
                    var messageJson = JsonSerializer.Serialize(transactionProcessedEvent);
                    
                    await messagePublisher.PublishAsync(
                        exchange: exchange,
                        routingKey: "transaction.processed",
                        message: messageJson);

                    logger.LogInformation("Transaction {TransactionCode} processed. Success: {Success}", transactionDto.Code, success);
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Failed to deserialize TransactionDto: {Message}", message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error processing transaction event");
                }
            });
    }
}
