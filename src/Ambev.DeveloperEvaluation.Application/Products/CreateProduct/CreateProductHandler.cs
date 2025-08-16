using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.CQRS;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Products.CreateProduct;

public class CreateProductHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(
        IProductRepository productRepository,
        IEventBus eventBus,
        ILogger<CreateProductHandler> logger)
    {
        _productRepository = productRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product with title: {Title}", request.Title);

        var product = new Product(
            request.Title,
            request.Price,
            request.Description,
            request.Category,
            request.Image);

        if (request.Rating.Rate > 0 || request.Rating.Count > 0)
        {
            product.UpdateRating(request.Rating.Rate, request.Rating.Count);
        }

        await _productRepository.CreateAsync(product, cancellationToken);

       var productCreatedEvent = new
        {
            ProductId = product.Id,
            Title = product.Title,
            Category = product.Category,
            Price = product.Price,
            CreatedAt = product.CreatedAt
        };

        await _eventBus.PublishAsync(productCreatedEvent);
        
        _logger.LogInformation("Product {ProductId} created successfully with title: {Title}", 
            product.Id, request.Title);

        return product.Id;
    }
}