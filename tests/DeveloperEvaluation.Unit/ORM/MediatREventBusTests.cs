using DeveloperEvaluation.ORM.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DeveloperEvaluation.Unit.ORM;

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
        _logger.ReceivedWithAnyArgs(2).LogInformation(default);
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ShouldNotThrow()
    {
        // Arrange
        TestEvent nullEvent = null;

        // Act - JsonSerializer.Serialize will serialize null as "null"
        await _eventBus.PublishAsync(nullEvent);

        // Assert - No mediator call should happen since null doesn't implement INotification
        await _mediator.DidNotReceive().Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_WhenMediatorThrows_ShouldLogError()
    {
        // Arrange
        var testEvent = new TestEvent { Id = Guid.NewGuid(), Name = "Test Event" };
        var exception = new InvalidOperationException("Mediator error");

        _mediator.When(x => x.Publish(testEvent, Arg.Any<CancellationToken>()))
                 .Do(x => throw exception);

        // Act & Assert - Should rethrow the exception
        await Assert.ThrowsAsync<InvalidOperationException>(() => _eventBus.PublishAsync(testEvent));
        
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task PublishAsync_WithStringEvent_ShouldLogWarning()
    {
        // Arrange
        var stringEvent = "Simple string event";

        // Act
        await _eventBus.PublishAsync(stringEvent);

    // Assert - String doesn't implement INotification, so should log info and warning
    await _mediator.DidNotReceive().Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    _logger.Received().Log(
        LogLevel.Information,
        Arg.Any<EventId>(),
        Arg.Any<object>(),
        null,
        Arg.Any<Func<object, Exception, string>>());
    _logger.Received().Log(
        LogLevel.Warning,
        Arg.Any<EventId>(),
        Arg.Any<object>(),
        null,
        Arg.Any<Func<object, Exception, string>>());
    }

    private class TestEvent : INotification
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}