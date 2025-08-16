using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.CQRS;
using Ambev.DeveloperEvaluation.ORM.Redis;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProducts;

public class ListProductsHandler : IQueryHandler<ListProductsQuery, ListProductsResult>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<ListProductsHandler> _logger;

    public ListProductsHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<ListProductsHandler> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ListProductsResult> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing products: Page={Page}, Limit={Limit}, Sort={Sort}, Category={Category}", 
            request.Page, request.Limit, request.Sort, request.Category);

        var cacheKey = $"products:page-{request.Page}:limit-{request.Limit}:sort-{request.Sort}:category-{request.Category}";
        var cachedResult = await _cacheService.GetAsync<ListProductsResult>(cacheKey);
        
        if (cachedResult != null)
        {
            _logger.LogInformation("Products list found in cache");
            return cachedResult;
        }

        var paginatedProducts = string.IsNullOrEmpty(request.Category)
            ? await _productRepository.GetAllAsync(request.Page, request.Limit, request.Sort, cancellationToken)
            : await _productRepository.GetByCategoryAsync(request.Category, request.Page, request.Limit, request.Sort, cancellationToken);

        var result = new ListProductsResult
        {
            Products = _mapper.Map<List<GetProductResult>>(paginatedProducts),
            TotalCount = paginatedProducts.TotalCount,
            Page = request.Page,
            PageSize = request.Limit
        };
        
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
        
        _logger.LogInformation("Retrieved {ProductCount} products out of {TotalCount}", 
            result.Products.Count, result.TotalCount);
            
        return result;
    }
}