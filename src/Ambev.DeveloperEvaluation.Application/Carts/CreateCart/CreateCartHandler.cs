using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.CQRS;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

public class CreateCartHandler : ICommandHandler<CreateCartCommand, Guid>
{
    private readonly ICartRepository _cartRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateCartHandler> _logger;

    public CreateCartHandler(
        ICartRepository cartRepository,
        IEventBus eventBus,
        ILogger<CreateCartHandler> logger)
    {
        _cartRepository = cartRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating cart for user {UserId} with {ProductCount} products", 
            request.UserId, request.Products.Count);

        // Check if user already has a cart
        var existingCart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existingCart != null)
        {
            _logger.LogWarning("User {UserId} already has an existing cart {CartId}", 
                request.UserId, existingCart.Id);
            throw new InvalidOperationException($"User {request.UserId} already has an active cart");
        }

        var cart = new Cart(request.UserId);
        
        foreach (var productRequest in request.Products)
        {
            cart.AddProduct(productRequest.ProductId, productRequest.Quantity);
        }

        await _cartRepository.CreateAsync(cart, cancellationToken);

        // Publish domain event
        var cartCreatedEvent = new
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            ProductCount = cart.Products.Count,
            CreatedAt = cart.CreatedAt
        };

        await _eventBus.PublishAsync(cartCreatedEvent);
        
        _logger.LogInformation("Cart {CartId} created successfully for user {UserId}", 
            cart.Id, request.UserId);

        return cart.Id;
    }
}