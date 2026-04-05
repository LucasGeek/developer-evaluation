using DeveloperEvaluation.Application.Carts.DeleteCart;
using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Domain.Repositories;
using DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DeveloperEvaluation.Unit.Application;

public class DeleteCartHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<DeleteCartHandler> _logger;
    private readonly DeleteCartHandler _handler;

    public DeleteCartHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _eventBus = Substitute.For<IEventBus>();
        _logger = Substitute.For<ILogger<DeleteCartHandler>>();
        _handler = new DeleteCartHandler(_cartRepository, _eventBus, _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteCartSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cartId = Guid.NewGuid();

        var existingCart = new Cart(userId);
        existingCart.Id = cartId;

        var command = new DeleteCartCommand(cartId, userId);

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>())
            .Returns(existingCart);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        await _cartRepository.Received(1).GetByIdAsync(cartId, Arg.Any<CancellationToken>());
        await _cartRepository.Received(1).DeleteAsync(cartId, Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublishAsync(Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithNonExistentCart_ShouldReturnFalse()
    {
        // Arrange
        var command = new DeleteCartCommand(Guid.NewGuid(), Guid.NewGuid());

        _cartRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Cart?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        await _cartRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _cartRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _eventBus.DidNotReceive().PublishAsync(Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithDifferentUserId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var cartUserId = Guid.NewGuid();
        var requestUserId = Guid.NewGuid();
        var cartId = Guid.NewGuid();

        var existingCart = new Cart(cartUserId);
        typeof(Cart).GetField("_id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(existingCart, cartId);

        var command = new DeleteCartCommand(cartId, requestUserId);

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>())
            .Returns(existingCart);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("User cannot delete cart belonging to another user", exception.Message);
        await _cartRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var existingCart = new Cart(userId);
        existingCart.Id = cartId;

        var command = new DeleteCartCommand(cartId, userId);

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>())
            .Returns(existingCart);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify the important operations were called
        await _cartRepository.Received(1).GetByIdAsync(cartId, Arg.Any<CancellationToken>());
        await _cartRepository.Received(1).DeleteAsync(cartId, Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublishAsync(Arg.Any<object>());
    }
}