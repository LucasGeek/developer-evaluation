using DeveloperEvaluation.Domain.Repositories;
using DeveloperEvaluation.ORM.CQRS;
using DeveloperEvaluation.ORM.Redis;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Products.GetCategories;

public class GetCategoriesHandler : IQueryHandler<GetCategoriesQuery, GetCategoriesResult>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetCategoriesHandler> _logger;

    public GetCategoriesHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        ILogger<GetCategoriesHandler> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GetCategoriesResult> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product categories");

        const string cacheKey = "product-categories";
        var cachedCategories = await _cacheService.GetAsync<GetCategoriesResult>(cacheKey);
        
        if (cachedCategories != null)
        {
            _logger.LogInformation("Categories found in cache");
            return cachedCategories;
        }

        var categories = await _productRepository.GetCategoriesAsync(cancellationToken);
        var result = new GetCategoriesResult
        {
            Categories = categories.ToList()
        };
        
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
        
        _logger.LogInformation("Retrieved {CategoryCount} categories", result.Categories.Count);
        return result;
    }
}