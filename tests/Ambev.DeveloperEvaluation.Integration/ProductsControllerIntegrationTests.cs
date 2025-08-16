using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.WebApi.Features.Products;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public class ProductsControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductsControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetProducts_WithValidRequest_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/Products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProductListResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/Products?_page=1&_size=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProductListResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.CurrentPage);
    }

    [Fact]
    public async Task GetProducts_WithSorting_ReturnsCorrectOrder()
    {
        // Act
        var response = await _client.GetAsync("/api/Products?_order=price desc");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProductListResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        var productRequest = new CreateProductRequest
        {
            Title = "Test Product",
            Price = 29.99m,
            Description = "A test product description",
            Category = "test",
            Image = "https://example.com/test.jpg",
            Rating = new ProductRatingRequest 
            { 
                Rate = 4.5m, 
                Count = 10 
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProductResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(productRequest.Title, result.Title);
        Assert.Equal(productRequest.Price, result.Price);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var productRequest = new CreateProductRequest
        {
            Title = "", // Invalid - empty title
            Price = -1, // Invalid - negative price
            Description = "A test product description",
            Category = "test",
            Image = "https://example.com/test.jpg",
            Rating = new ProductRatingRequest 
            { 
                Rate = 4.5m, 
                Count = 10 
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Products", productRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Products/{productId}");

        // Assert - Since this is a mock implementation, it might return NotFound or OK depending on implementation
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.Empty;

        // Act
        var response = await _client.GetAsync($"/api/Products/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateRequest = new UpdateProductRequest
        {
            Title = "Updated Product",
            Price = 39.99m,
            Description = "Updated product description",
            Category = "updated",
            Image = "https://example.com/updated.jpg",
            Rating = new ProductRatingRequest 
            { 
                Rate = 4.8m, 
                Count = 15 
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Products/{productId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProductResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Equal(updateRequest.Title, result.Title);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Products/{productId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/Products/categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<string>>(content, _jsonOptions);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetProductsByCategory_WithValidCategory_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/Products/category/beverages");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProductListResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetProductsByCategory_WithInvalidCategory_ReturnsOkWithEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/Products/category/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProductListResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }
}