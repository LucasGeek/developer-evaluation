using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.CQRS;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

public class UpdateCartHandler : ICommandHandler<UpdateCartCommand, bool>
{
    private readonly ICartRepository _cartRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<UpdateCartHandler> _logger;

    public UpdateCartHandler(
        ICartRepository cartRepository,
        IEventBus eventBus,
        ILogger<UpdateCartHandler> logger)
    {
        _cartRepository = cartRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating cart {CartId} for user {UserId}", 
            request.Id, request.UserId);

        var cart = await _cartRepository.GetByIdAsync(request.Id, cancellationToken);
        if (cart == null)
        {
            _logger.LogWarning("Cart {CartId} not found", request.Id);
            return false;
        }

        if (cart.UserId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to update cart {CartId} belonging to user {CartUserId}", 
                request.UserId, request.Id, cart.UserId);
            throw new InvalidOperationException("User cannot update cart belonging to another user");
        }

        // Clear existing products and add new ones
        cart.Clear();
        
        foreach (var productRequest in request.Products)
        {
            cart.AddProduct(productRequest.ProductId, productRequest.Quantity);
        }

        await _cartRepository.UpdateAsync(cart, cancellationToken);

        // Publish domain event
        var cartUpdatedEvent = new
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            ProductCount = cart.Products.Count,
            UpdatedAt = cart.UpdatedAt
        };

        await _eventBus.PublishAsync(cartUpdatedEvent);
        
        _logger.LogInformation("Cart {CartId} updated successfully", cart.Id);

        return true;
    }
}