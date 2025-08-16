using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public class CartsControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public CartsControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetCarts_WithValidRequest_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/Carts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CartListResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetCarts_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/Carts?_page=1&_size=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CartListResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.CurrentPage);
    }

    [Fact]
    public async Task CreateCart_WithValidData_ReturnsCreated()
    {
        // Arrange
        var cartRequest = new CreateCartRequest
        {
            UserId = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Products = new List<CartItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 },
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Carts", cartRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CartResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(cartRequest.UserId, result.UserId);
        Assert.Equal(2, result.Products.Count);
    }

    [Fact]
    public async Task CreateCart_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var cartRequest = new CreateCartRequest
        {
            UserId = Guid.Empty, // Invalid
            Date = DateTime.UtcNow,
            Products = new List<CartItemRequest>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Carts", cartRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCart_WithValidId_ReturnsOk()
    {
        // Arrange - First create a cart
        var cartRequest = new CreateCartRequest
        {
            UserId = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Products = new List<CartItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Carts", cartRequest);
        var createdCart = JsonSerializer.Deserialize<CartResponse>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/Carts/{createdCart!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CartResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Equal(createdCart.Id, result.Id);
    }

    [Fact]
    public async Task GetCart_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Carts/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCart_WithValidData_ReturnsOk()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var updateRequest = new CreateCartRequest
        {
            UserId = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Products = new List<CartItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 3 }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Carts/{cartId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CartResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Equal(cartId, result.Id);
    }

    [Fact]
    public async Task DeleteCart_WithValidId_ReturnsOk()
    {
        // Arrange
        var cartId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Carts/{cartId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}