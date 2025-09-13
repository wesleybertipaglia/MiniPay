namespace Shared.Core.Interface;

public interface IMessageConsumer
{
    Task ConsumeAsync(string exchange, string queue, string routingKey, Func<string, Task> onMessageReceived);
}
