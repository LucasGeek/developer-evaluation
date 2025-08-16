using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts;
using Ambev.DeveloperEvaluation.WebApi.Common;
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
        var apiResponse = JsonSerializer.Deserialize<ApiResponseWithData<CartListResponse>>(content, _jsonOptions);
        
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Data);
        Assert.NotNull(apiResponse.Data.Data);
    }

    [Fact]
    public async Task GetCarts_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/Carts?_page=1&_size=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponseWithData<CartListResponse>>(content, _jsonOptions);
        
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(1, apiResponse.Data.CurrentPage);
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
        var cartResponse = JsonSerializer.Deserialize<CartResponse>(content, _jsonOptions);
        
        Assert.NotNull(cartResponse);
        Assert.NotEqual(Guid.Empty, cartResponse.Id);
        Assert.Equal(cartRequest.UserId, cartResponse.UserId);
        Assert.Equal(2, cartResponse.Products.Count);
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
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdCart = JsonSerializer.Deserialize<CartResponse>(createContent, _jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/Carts/{createdCart!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponseWithData<CartResponse>>(content, _jsonOptions);
        
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(createdCart.Id, apiResponse.Data.Id);
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
        var apiResponse = JsonSerializer.Deserialize<ApiResponseWithData<CartResponse>>(content, _jsonOptions);
        
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(cartId, apiResponse.Data.Id);
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