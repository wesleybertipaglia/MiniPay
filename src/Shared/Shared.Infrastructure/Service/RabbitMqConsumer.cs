using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
        logger.LogInformation("Initializing consumer for exchange '{Exchange}', queue '{Queue}', routingKey '{RoutingKey}'",
            exchange, queue, routingKey);

        try
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

                logger.LogInformation("Message received from queue '{Queue}' with delivery tag {DeliveryTag}. Message: {Message}",
                    queue, ea.DeliveryTag, message);

                try
                {
                    await onMessageReceived(message);
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);

                    logger.LogInformation("Message acknowledged (DeliveryTag: {DeliveryTag})", ea.DeliveryTag);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Error processing message from queue '{Queue}' (DeliveryTag: {DeliveryTag}). Requeuing message.",
                        queue, ea.DeliveryTag);

                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await _channel.BasicConsumeAsync(queue: queue, autoAck: false, consumer: consumer);

            logger.LogInformation("Successfully subscribed to queue '{Queue}' on exchange '{Exchange}' with routing key '{RoutingKey}'",
                queue, exchange, routingKey);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to initialize RabbitMQ consumer for exchange '{Exchange}', queue '{Queue}', routingKey '{RoutingKey}'",
                exchange, queue, routingKey);
            throw;
        }
    }
}
