using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Core.Helpers;
using Shared.Core.Interface;

namespace Shared.Infrastructure.Service;

public class RabbitMqPublisher(ILogger<RabbitMqPublisher> logger) : IMessagePublisher
{
    private const string HostName = "localhost";

    public async Task PublishAsync(string exchange, string routingKey, string message)
    {
        var factory = new ConnectionFactory { HostName = HostName };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: exchange, type: ExchangeType.Direct, durable: true);

        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: new BasicProperties { Persistent = true },
            body: body
        );
        
        LogHelper.LogInfo(logger, "Message published to exchange '{Exchange}' with routingKey '{RoutingKey}': {Message}", [exchange, routingKey, message]);
    }
}