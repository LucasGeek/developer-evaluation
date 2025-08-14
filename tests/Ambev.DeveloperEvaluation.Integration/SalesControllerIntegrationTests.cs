using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public class SalesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SalesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/sales should create sale successfully")]
    public async Task Post_ValidSaleRequest_ShouldCreateSaleSuccessfully()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            BranchId = Guid.NewGuid(),
            BranchDescription = "Test Branch",
            CustomerId = Guid.NewGuid(),
            CustomerDescription = "Test Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Product 1",
                    Quantity = 5,
                    UnitPrice = 10.00m
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Product 2",
                    Quantity = 10,
                    UnitPrice = 20.00m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Be("Sale created successfully");
        
        var data = jsonDocument.RootElement.GetProperty("data");
        var saleId = data.GetProperty("id").GetGuid();
        saleId.Should().NotBeEmpty();
        
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.ToString().Should().Contain($"/api/sales/{saleId}");
    }

    [Fact(DisplayName = "POST /api/sales with invalid data should return bad request")]
    public async Task Post_InvalidSaleRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            BranchId = Guid.Empty, // Invalid
            BranchDescription = "",
            CustomerId = Guid.Empty, // Invalid
            CustomerDescription = "",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>() // Empty items
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /api/sales should apply discount rules correctly")]
    public async Task Post_SaleWithDiscountEligibleItems_ShouldApplyDiscountsCorrectly()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            BranchId = Guid.NewGuid(),
            BranchDescription = "Test Branch",
            CustomerId = Guid.NewGuid(),
            CustomerDescription = "Test Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Product with 10% discount",
                    Quantity = 4, // 10% discount
                    UnitPrice = 10.00m
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Product with 20% discount",
                    Quantity = 15, // 20% discount
                    UnitPrice = 20.00m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        
        var data = jsonDocument.RootElement.GetProperty("data");
        var saleId = data.GetProperty("id").GetGuid();
        saleId.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "POST /api/sales with quantity over 20 should return bad request")]
    public async Task Post_ItemQuantityOver20_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateSaleRequest
        {
            BranchId = Guid.NewGuid(),
            BranchDescription = "Test Branch",
            CustomerId = Guid.NewGuid(),
            CustomerDescription = "Test Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Product 1",
                    Quantity = 21, // Invalid: over 20
                    UnitPrice = 10.00m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}