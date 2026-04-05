using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DeveloperEvaluation.ORM.Messaging;

public class LoggingEventBus : IEventBus
{
    private readonly ILogger<LoggingEventBus> _logger;

    public LoggingEventBus(ILogger<LoggingEventBus> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<T>(T eventMessage) where T : class
    {
        var eventName = typeof(T).Name;
        var eventData = JsonSerializer.Serialize(eventMessage, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        _logger.LogInformation(
            "Domain Event Published: {EventName} - {EventData}", 
            eventName, 
            eventData);

        await Task.CompletedTask;
    }

    public async Task SendAsync<T>(T command) where T : class
    {
        var commandName = typeof(T).Name;
        var commandData = JsonSerializer.Serialize(command, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        _logger.LogInformation(
            "Command Sent: {CommandName} - {CommandData}", 
            commandName, 
            commandData);

        await Task.CompletedTask;
    }
}