using DeveloperEvaluation.Domain.Repositories;
using DeveloperEvaluation.ORM.CQRS;
using DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Carts.UpdateCart;

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

        cart.Clear();
        
        foreach (var productRequest in request.Products)
        {
            cart.AddProduct(productRequest.ProductId, productRequest.Quantity);
        }

        await _cartRepository.UpdateAsync(cart, cancellationToken);

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