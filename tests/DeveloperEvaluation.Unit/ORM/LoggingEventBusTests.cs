using DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DeveloperEvaluation.Unit.ORM;

public class LoggingEventBusTests
{
    private readonly ILogger<LoggingEventBus> _logger;
    private readonly LoggingEventBus _eventBus;

    public LoggingEventBusTests()
    {
        _logger = Substitute.For<ILogger<LoggingEventBus>>();
        _eventBus = new LoggingEventBus(_logger);
    }

    [Fact]
    public async Task PublishAsync_WithValidEvent_ShouldLogEvent()
    {
        // Arrange
        var testEvent = new { Id = Guid.NewGuid(), Name = "Test Event", Timestamp = DateTime.UtcNow };

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert - Just check that LogInformation was called
        _logger.ReceivedWithAnyArgs(1).LogInformation(default);
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ShouldLogEvent()
    {
        // Arrange
        object nullEvent = null;

        // Act
        await _eventBus.PublishAsync(nullEvent);

        // Assert - LoggingEventBus will serialize null as "null" and log it
        _logger.ReceivedWithAnyArgs(1).LogInformation(default);
    }

    [Fact]
    public async Task PublishAsync_WithStringEvent_ShouldLogEvent()
    {
        // Arrange
        var stringEvent = "Simple string event";

        // Act
        await _eventBus.PublishAsync(stringEvent);

        // Assert - Just check that LogInformation was called
        _logger.ReceivedWithAnyArgs(1).LogInformation(default);
    }

    [Fact]
    public async Task PublishAsync_WithComplexEvent_ShouldLogEvent()
    {
        // Arrange
        var complexEvent = new TestComplexEvent
        {
            Id = Guid.NewGuid(),
            Name = "Complex Event",
            Data = new { Key = "Value", Number = 42 },
            Tags = new[] { "tag1", "tag2" }
        };

        // Act
        await _eventBus.PublishAsync(complexEvent);

        // Assert - Just check that LogInformation was called
        _logger.ReceivedWithAnyArgs(1).LogInformation(default);
    }

    private class TestComplexEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
    }
}