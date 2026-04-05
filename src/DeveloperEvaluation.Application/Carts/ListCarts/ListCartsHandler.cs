using DeveloperEvaluation.Application.Carts.GetCart;
using DeveloperEvaluation.Domain.Repositories;
using DeveloperEvaluation.ORM.CQRS;
using DeveloperEvaluation.ORM.Redis;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Carts.ListCarts;

public class ListCartsHandler : IQueryHandler<ListCartsQuery, ListCartsResult>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<ListCartsHandler> _logger;

    public ListCartsHandler(
        ICartRepository cartRepository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<ListCartsHandler> logger)
    {
        _cartRepository = cartRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ListCartsResult> Handle(ListCartsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing carts - Page: {Page}, Limit: {Limit}, Sort: {Sort}", 
            request.Page, request.Limit, request.Sort);

        var cacheKey = $"carts:page:{request.Page}:limit:{request.Limit}:sort:{request.Sort}:start:{request.StartDate}:end:{request.EndDate}:startUser:{request.StartUserId}:endUser:{request.EndUserId}";
        var cachedResult = await _cacheService.GetAsync<ListCartsResult>(cacheKey);

        if (cachedResult != null)
        {
            _logger.LogInformation("Cached cart list found for page {Page}", request.Page);
            return cachedResult;
        }

        var paginatedCarts = request.StartUserId.HasValue && request.EndUserId.HasValue
            ? await _cartRepository.GetByUserIdRangeAsync(
                request.StartUserId.Value, 
                request.EndUserId.Value, 
                request.Page, 
                request.Limit, 
                request.Sort, 
                cancellationToken)
            : await _cartRepository.GetAllAsync(
                request.Page, 
                request.Limit, 
                request.Sort, 
                request.StartDate, 
                request.EndDate, 
                cancellationToken);

        var cartResults = _mapper.Map<List<GetCartResult>>(paginatedCarts.ToList());

        var result = new ListCartsResult
        {
            Carts = cartResults,
            TotalCount = paginatedCarts.TotalCount,
            Page = request.Page,
            PageSize = request.Limit
        };

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        _logger.LogInformation("Retrieved {CartCount} carts from {TotalCount} total carts", 
            cartResults.Count, paginatedCarts.TotalCount);

        return result;
    }
}