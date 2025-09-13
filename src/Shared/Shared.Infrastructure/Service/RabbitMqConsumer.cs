using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Core.Helpers;
using Shared.Core.Interface;

namespace Shared.Infrastructure.Service;

public class RabbitMqConsumer(ILogger<RabbitMqConsumer> logger) : IMessageConsumer
{
    private const string HostName = "localhost";
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task ConsumeAsync(
        string exchange,
        string queue,
        string routingKey,
        Func<string, Task> onMessageReceived)
    {
        var factory = new ConnectionFactory { HostName = HostName };
        _connection ??= await factory.CreateConnectionAsync();
        _channel ??= await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true);
        
        await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
        
        await _channel.QueueBindAsync(queue: queue, exchange: exchange, routingKey: routingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                await onMessageReceived(message);
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(logger, ex, "Error processing message");
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(queue: queue, autoAck: false, consumer: consumer);
        LogHelper.LogInfo(logger, "Subscribed to exchange '{Exchange}' with queue '{Queue}' and routingKey '{RoutingKey}'", [exchange, queue, routingKey]);
    }
}