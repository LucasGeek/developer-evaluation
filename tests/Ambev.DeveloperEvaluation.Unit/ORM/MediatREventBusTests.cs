using Ambev.DeveloperEvaluation.ORM.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

public class MediatREventBusTests
{
    private readonly IMediator _mediator;
    private readonly ILogger<MediatREventBus> _logger;
    private readonly MediatREventBus _eventBus;

    public MediatREventBusTests()
    {
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<MediatREventBus>>();
        _eventBus = new MediatREventBus(_mediator, _logger);
    }

    [Fact]
    public async Task PublishAsync_WithValidEvent_ShouldPublishThroughMediator()
    {
        // Arrange
        var testEvent = new TestEvent { Id = Guid.NewGuid(), Name = "Test Event" };

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        await _mediator.Received(1).Publish(testEvent, Arg.Any<CancellationToken>());
        _logger.Received(1).LogInformation(
            "Event published via MediatR: {EventType}",
            nameof(TestEvent));
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ShouldLogWarning()
    {
        // Arrange
        object nullEvent = null;

        // Act
        await _eventBus.PublishAsync(nullEvent);

        // Assert
        await _mediator.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
        _logger.Received(1).LogWarning("Attempted to publish null event via MediatR");
    }

    [Fact]
    public async Task PublishAsync_WhenMediatorThrows_ShouldLogError()
    {
        // Arrange
        var testEvent = new TestEvent { Id = Guid.NewGuid(), Name = "Test Event" };
        var exception = new InvalidOperationException("Mediator error");

        _mediator.When(x => x.Publish(testEvent, Arg.Any<CancellationToken>()))
                 .Do(x => throw exception);

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        _logger.Received(1).LogError(exception,
            "Error publishing event via MediatR: {EventType}",
            nameof(TestEvent));
    }

    [Fact]
    public async Task PublishAsync_WithStringEvent_ShouldPublishThroughMediator()
    {
        // Arrange
        var stringEvent = "Simple string event";

        // Act
        await _eventBus.PublishAsync(stringEvent);

        // Assert
        await _mediator.Received(1).Publish(stringEvent, Arg.Any<CancellationToken>());
        _logger.Received(1).LogInformation(
            "Event published via MediatR: {EventType}",
            "String");
    }

    private class TestEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}