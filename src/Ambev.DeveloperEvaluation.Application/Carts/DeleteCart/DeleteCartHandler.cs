using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.CQRS;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;

public class DeleteCartHandler : ICommandHandler<DeleteCartCommand, bool>
{
    private readonly ICartRepository _cartRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<DeleteCartHandler> _logger;

    public DeleteCartHandler(
        ICartRepository cartRepository,
        IEventBus eventBus,
        ILogger<DeleteCartHandler> logger)
    {
        _cartRepository = cartRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCartCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting cart {CartId} for user {UserId}", 
            request.Id, request.UserId);

        var cart = await _cartRepository.GetByIdAsync(request.Id, cancellationToken);
        if (cart == null)
        {
            _logger.LogWarning("Cart {CartId} not found", request.Id);
            return false;
        }

        if (cart.UserId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to delete cart {CartId} belonging to user {CartUserId}", 
                request.UserId, request.Id, cart.UserId);
            throw new InvalidOperationException("User cannot delete cart belonging to another user");
        }

        await _cartRepository.DeleteAsync(request.Id, cancellationToken);

        // Publish domain event
        var cartDeletedEvent = new
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            DeletedAt = DateTime.UtcNow
        };

        await _eventBus.PublishAsync(cartDeletedEvent);
        
        _logger.LogInformation("Cart {CartId} deleted successfully", request.Id);

        return true;
    }
}