using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Core.Interface;

namespace Shared.Infrastructure.Service;

public class RabbitMqPublisher(ILogger<RabbitMqPublisher> logger) : IMessagePublisher
{
    private const string HostName = "localhost";

    public async Task PublishAsync(string exchange, string routingKey, string message)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = HostName };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: exchange,
                type: ExchangeType.Topic,
                durable: true
            );

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: new BasicProperties { Persistent = true },
                body: body
            );

            logger.LogInformation(
                "Message published to exchange '{Exchange}' with routing key '{RoutingKey}': {Message}",
                exchange, routingKey, message
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to publish message to exchange '{Exchange}' with routing key '{RoutingKey}'. Message: {Message}",
                exchange, routingKey, message
            );
            throw;
        }
    }
}