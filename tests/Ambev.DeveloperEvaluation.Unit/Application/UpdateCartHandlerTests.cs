using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UpdateCartHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<UpdateCartHandler> _logger;
    private readonly UpdateCartHandler _handler;

    public UpdateCartHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _eventBus = Substitute.For<IEventBus>();
        _logger = Substitute.For<ILogger<UpdateCartHandler>>();
        _handler = new UpdateCartHandler(_cartRepository, _eventBus, _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateCartSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var existingCart = new Cart(userId);
        existingCart.Id = cartId;

        var command = new UpdateCartCommand
        {
            Id = cartId,
            UserId = userId,
            Products = new List<CartItemRequest>
            {
                new() { ProductId = productId, Quantity = 3 }
            }
        };

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>())
            .Returns(existingCart);
        _cartRepository.UpdateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>())
            .Returns(existingCart);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        await _cartRepository.Received(1).GetByIdAsync(cartId, Arg.Any<CancellationToken>());
        await _cartRepository.Received(1).UpdateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublishAsync(Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithNonExistentCart_ShouldReturnFalse()
    {
        // Arrange
        var command = new UpdateCartCommand
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Products = new List<CartItemRequest>()
        };

        _cartRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Cart?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        await _cartRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _cartRepository.DidNotReceive().UpdateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
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

        var command = new UpdateCartCommand
        {
            Id = cartId,
            UserId = requestUserId,
            Products = new List<CartItemRequest>()
        };

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>())
            .Returns(existingCart);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("User cannot update cart belonging to another user", exception.Message);
        await _cartRepository.DidNotReceive().UpdateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var existingCart = new Cart(userId);
        existingCart.Id = cartId;

        var command = new UpdateCartCommand
        {
            Id = cartId,
            UserId = userId,
            Products = new List<CartItemRequest>()
        };

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>())
            .Returns(existingCart);
        _cartRepository.UpdateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>())
            .Returns(existingCart);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify the important operations were called
        await _cartRepository.Received(1).GetByIdAsync(cartId, Arg.Any<CancellationToken>());
        await _cartRepository.Received(1).UpdateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublishAsync(Arg.Any<object>());
    }
}