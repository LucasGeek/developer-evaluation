using DeveloperEvaluation.Domain.Repositories;
using DeveloperEvaluation.ORM.CQRS;
using DeveloperEvaluation.ORM.Redis;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Carts.GetCart;

public class GetCartHandler : IQueryHandler<GetCartQuery, GetCartResult?>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCartHandler> _logger;

    public GetCartHandler(
        ICartRepository cartRepository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<GetCartHandler> logger)
    {
        _cartRepository = cartRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GetCartResult?> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting cart with ID {CartId}", request.Id);

        var cacheKey = $"cart:{request.Id}";
        var cachedCart = await _cacheService.GetAsync<GetCartResult>(cacheKey);

        if (cachedCart != null)
        {
            _logger.LogInformation("Cart {CartId} found in cache", request.Id);
            return cachedCart;
        }

        var cart = await _cartRepository.GetByIdAsync(request.Id, cancellationToken);
        if (cart == null)
        {
            _logger.LogWarning("Cart with ID {CartId} not found", request.Id);
            return null;
        }

        var result = _mapper.Map<GetCartResult>(cart);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));

        _logger.LogInformation("Cart {CartId} retrieved successfully with {ProductCount} products", 
            request.Id, result.Products.Count);
        
        return result;
    }
}