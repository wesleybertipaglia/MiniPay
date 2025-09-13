namespace Shared.Core.Interface;

public interface IMessagePublisher
{
    Task PublishAsync(string exchange, string routingKey, string message);
}
