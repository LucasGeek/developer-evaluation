using DeveloperEvaluation.Domain.Repositories;
using DeveloperEvaluation.ORM.CQRS;
using DeveloperEvaluation.ORM.Redis;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Products.GetProduct;

public class GetProductHandler : IQueryHandler<GetProductQuery, GetProductResult?>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductHandler> _logger;

    public GetProductHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<GetProductHandler> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GetProductResult?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product with ID {ProductId}", request.Id);

        var cacheKey = $"product:{request.Id}";
        var cachedProduct = await _cacheService.GetAsync<GetProductResult>(cacheKey);
        
        if (cachedProduct != null)
        {
            _logger.LogInformation("Product {ProductId} found in cache", request.Id);
            return cachedProduct;
        }

        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", request.Id);
            return null;
        }

        var result = _mapper.Map<GetProductResult>(product);
        
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
        
        _logger.LogInformation("Product {ProductId} retrieved successfully", request.Id);
        return result;
    }
}