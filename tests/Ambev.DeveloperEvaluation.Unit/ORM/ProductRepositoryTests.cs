using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

public class ProductRepositoryTests : IDisposable
{
    private readonly DefaultContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DefaultContext(options);
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_WithValidProduct_ShouldCreateSuccessfully()
    {
        // Arrange
        var product = new Product(
            "Test Product",
            29.99m,
            "A test product",
            "Electronics",
            "https://example.com/image.jpg");

        // Act
        var result = await _repository.CreateAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(product.Title, result.Title);
        Assert.Equal(product.Price, result.Price);

        var createdProduct = await _context.Products.FindAsync(result.Id);
        Assert.NotNull(createdProduct);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingProduct_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product(
            "Test Product",
            29.99m,
            "A test product",
            "Electronics",
            "https://example.com/image.jpg");

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Title, result.Title);
        Assert.Equal(product.Price, result.Price);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentProduct_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCategoryAsync_WithExistingCategory_ShouldReturnProducts()
    {
        // Arrange
        var product1 = new Product("Product 1", 10.00m, "Description 1", "Electronics", "image1.jpg");
        var product2 = new Product("Product 2", 20.00m, "Description 2", "Electronics", "image2.jpg");
        var product3 = new Product("Product 3", 30.00m, "Description 3", "Clothing", "image3.jpg");

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync("Electronics", 1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal("Electronics", p.Category));
    }

    [Fact]
    public async Task GetCategoriesAsync_WithProducts_ShouldReturnUniqueCategories()
    {
        // Arrange
        var products = new[]
        {
            new Product("Product 1", 10.00m, "Description 1", "Electronics", "image1.jpg"),
            new Product("Product 2", 20.00m, "Description 2", "Electronics", "image2.jpg"),
            new Product("Product 3", 30.00m, "Description 3", "Clothing", "image3.jpg"),
            new Product("Product 4", 40.00m, "Description 4", "Books", "image4.jpg")
        };

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Contains("Electronics", result);
        Assert.Contains("Clothing", result);
        Assert.Contains("Books", result);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingProduct_ShouldUpdateSuccessfully()
    {
        // Arrange
        var product = new Product(
            "Test Product",
            29.99m,
            "A test product",
            "Electronics",
            "https://example.com/image.jpg");

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act - just test that UpdateAsync returns the same product
        var result = await _repository.UpdateAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Title, result.Title);
        Assert.Equal(product.Price, result.Price);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingProduct_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product(
            "Test Product",
            29.99m,
            "A test product",
            "Electronics",
            "https://example.com/image.jpg");

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(product.Id);

        // Assert
        var deletedProduct = await _context.Products.FindAsync(product.Id);
        Assert.Null(deletedProduct);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentProduct_ShouldNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert - should not throw any exception
        await _repository.DeleteAsync(nonExistentId);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var products = new List<Product>();
        for (int i = 0; i < 5; i++)
        {
            products.Add(new Product(
                $"Product {i}",
                10.00m * (i + 1),
                $"Description {i}",
                "Electronics",
                $"image{i}.jpg"));
        }

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(1, 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.TotalPages);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}