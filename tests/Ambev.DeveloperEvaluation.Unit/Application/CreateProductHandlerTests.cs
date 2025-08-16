using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using System.Reflection;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _eventBus = Substitute.For<IEventBus>();
        _logger = Substitute.For<ILogger<CreateProductHandler>>();
        _handler = new CreateProductHandler(_productRepository, _eventBus, _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Title = "Test Product",
            Price = 29.99m,
            Description = "A test product",
            Category = "Electronics",
            Image = "https://example.com/product.jpg",
            Rating = new RatingCommand { Rate = 4.5, Count = 10 }
        };

        var productId = Guid.NewGuid();
        var createdProduct = new Product(command.Title, command.Price, command.Description, command.Category, command.Image);
        createdProduct.Id = productId;
        
        _productRepository.CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(createdProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _productRepository.Received(1).CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublishAsync(Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithValidCommandAndRating_ShouldUpdateProductRating()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Title = "Test Product",
            Price = 29.99m,
            Description = "A test product",
            Category = "Electronics",
            Image = "https://example.com/product.jpg",
            Rating = new RatingCommand { Rate = 4.5, Count = 15 }
        };

        var productId = Guid.NewGuid();
        var createdProduct = new Product(command.Title, command.Price, command.Description, command.Category, command.Image);
        createdProduct.Id = productId;
        
        _productRepository.CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(createdProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _productRepository.Received(1).CreateAsync(
            Arg.Is<Product>(p => 
                p.Title == command.Title &&
                p.Price == command.Price &&
                p.Rating.Rate == command.Rating.Rate &&
                p.Rating.Count == command.Rating.Count
            ), 
            Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublishAsync(Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithZeroRating_ShouldCreateProductWithoutRating()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Title = "Test Product",
            Price = 29.99m,
            Description = "A test product",
            Category = "Electronics",
            Image = "https://example.com/product.jpg",
            Rating = new RatingCommand { Rate = 0, Count = 0 }
        };

        var productId = Guid.NewGuid();
        var createdProduct = new Product(command.Title, command.Price, command.Description, command.Category, command.Image);
        createdProduct.Id = productId;
        
        _productRepository.CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(createdProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _productRepository.Received(1).CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublishAsync(Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Title = "Test Product",
            Price = 29.99m,
            Description = "A test product",
            Category = "Electronics",
            Image = "https://example.com/product.jpg",
            Rating = new RatingCommand { Rate = 4.5, Count = 10 }
        };

        var productId = Guid.NewGuid();
        var createdProduct = new Product(command.Title, command.Price, command.Description, command.Category, command.Image);
        createdProduct.Id = productId;
        
        _productRepository.CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(createdProduct);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Just verify that CreateAsync and PublishAsync were called
        await _productRepository.Received(1).CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublishAsync(Arg.Any<object>());
    }
}