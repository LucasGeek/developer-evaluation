using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

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

        // Assert
        _logger.Received(1).LogInformation(
            "Event published: {EventType} - {Event}",
            testEvent.GetType().Name,
            Arg.Any<object>());
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ShouldLogWarning()
    {
        // Arrange
        object nullEvent = null;

        // Act
        await _eventBus.PublishAsync(nullEvent);

        // Assert
        _logger.Received(1).LogWarning("Attempted to publish null event");
    }

    [Fact]
    public async Task PublishAsync_WithStringEvent_ShouldLogEvent()
    {
        // Arrange
        var stringEvent = "Simple string event";

        // Act
        await _eventBus.PublishAsync(stringEvent);

        // Assert
        _logger.Received(1).LogInformation(
            "Event published: {EventType} - {Event}",
            "String",
            stringEvent);
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

        // Assert
        _logger.Received(1).LogInformation(
            "Event published: {EventType} - {Event}",
            nameof(TestComplexEvent),
            Arg.Any<object>());
    }

    private class TestComplexEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
    }
}