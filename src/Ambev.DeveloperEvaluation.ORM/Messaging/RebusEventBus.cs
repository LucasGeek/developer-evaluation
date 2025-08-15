using Rebus.Bus;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.ORM.Messaging;

public class RebusEventBus : IEventBus
{
    private readonly IBus _bus;
    private readonly ILogger<RebusEventBus> _logger;

    public RebusEventBus(IBus bus, ILogger<RebusEventBus> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T @event) where T : class
    {
        try
        {
            _logger.LogInformation("Publishing event via Rebus: {EventType}", typeof(T).Name);
            await _bus.Publish(@event);
            _logger.LogInformation("Event published successfully via Rebus: {EventType}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event via Rebus: {EventType}", typeof(T).Name);
            throw;
        }
    }

    public async Task SendAsync<T>(T command) where T : class
    {
        try
        {
            _logger.LogInformation("Sending command via Rebus: {CommandType}", typeof(T).Name);
            await _bus.Send(command);
            _logger.LogInformation("Command sent successfully via Rebus: {CommandType}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send command via Rebus: {CommandType}", typeof(T).Name);
            throw;
        }
    }
}