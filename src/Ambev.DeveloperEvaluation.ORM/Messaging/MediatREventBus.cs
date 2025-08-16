using MediatR;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.ORM.Messaging;

public class MediatREventBus : IEventBus
{
    private readonly IMediator _mediator;
    private readonly ILogger<MediatREventBus> _logger;

    public MediatREventBus(IMediator mediator, ILogger<MediatREventBus> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T eventMessage) where T : class
    {
        try
        {
            var eventName = typeof(T).Name;
            
            var eventData = JsonSerializer.Serialize(eventMessage, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogInformation("Publishing Domain Event: {EventName} - {EventData}", eventName, eventData);

            if (eventMessage is INotification notification)
            {
                await _mediator.Publish(notification);
                _logger.LogInformation("Domain Event Published Successfully: {EventName}", eventName);
            }
            else
            {
                _logger.LogWarning("Event {EventName} does not implement INotification, only logged", eventName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing domain event: {EventType}", typeof(T).Name);
            throw;
        }
    }

    public async Task SendAsync<T>(T command) where T : class
    {
        try
        {
            var commandName = typeof(T).Name;
            
            var commandData = JsonSerializer.Serialize(command, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogInformation("Sending Command: {CommandName} - {CommandData}", commandName, commandData);

            if (command is IRequest request)
            {
                await _mediator.Send(request);
                _logger.LogInformation("Command Sent Successfully: {CommandName}", commandName);
            }
            else
            {
                _logger.LogWarning("Command {CommandName} does not implement IRequest, only logged", commandName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending command: {CommandType}", typeof(T).Name);
            throw;
        }
    }
}