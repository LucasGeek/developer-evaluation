using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.ORM.Messaging;

public class RebusEventBus : IEventBus
{
    private readonly IBus _bus;

    public RebusEventBus(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<T>(T @event) where T : class
    {
        await _bus.Publish(@event);
    }

    public async Task SendAsync<T>(T command) where T : class
    {
        await _bus.Send(command);
    }
}