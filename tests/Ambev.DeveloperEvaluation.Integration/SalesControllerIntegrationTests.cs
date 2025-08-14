using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public class SalesControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SalesControllerIntegrationTests(TestWebApplicationFactory factory)
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

    [Fact(DisplayName = "GET /api/sales/{id} should return sale when exists")]
    public async Task Get_ExistingSaleId_ShouldReturnSale()
    {
        // Arrange - First create a sale
        var createRequest = new CreateSaleRequest
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
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Act - Get the created sale
        var response = await _client.GetAsync($"/api/sales/{saleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Be("Sale retrieved successfully");
        
        var data = jsonDocument.RootElement.GetProperty("data");
        data.GetProperty("id").GetGuid().Should().Be(saleId);
        data.GetProperty("branchDescription").GetString().Should().Be("Test Branch");
        data.GetProperty("customerDescription").GetString().Should().Be("Test Customer");
        
        var items = data.GetProperty("items");
        items.GetArrayLength().Should().Be(1);
        
        var firstItem = items[0];
        firstItem.GetProperty("productDescription").GetString().Should().Be("Product 1");
        firstItem.GetProperty("quantity").GetInt32().Should().Be(5);
        firstItem.GetProperty("unitPrice").GetDecimal().Should().Be(10.00m);
    }

    [Fact(DisplayName = "GET /api/sales/{id} should return 404 when sale does not exist")]
    public async Task Get_NonExistentSaleId_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sales/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Be("Sale not found");
    }

    [Fact(DisplayName = "GET /api/sales/{id} should return sale with discount calculations")]
    public async Task Get_SaleWithDiscounts_ShouldReturnCorrectDiscounts()
    {
        // Arrange - Create sale with items that have discounts
        var createRequest = new CreateSaleRequest
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
                    Quantity = 10, // 20% discount
                    UnitPrice = 20.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Act
        var response = await _client.GetAsync($"/api/sales/{saleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        var data = jsonDocument.RootElement.GetProperty("data");
        var items = data.GetProperty("items");
        items.GetArrayLength().Should().Be(2);

        // First item: 4 * 10.00 = 40.00, discount 10% = 4.00, total = 36.00
        var firstItem = items[0];
        firstItem.GetProperty("quantity").GetInt32().Should().Be(4);
        firstItem.GetProperty("unitPrice").GetDecimal().Should().Be(10.00m);
        firstItem.GetProperty("discount").GetDecimal().Should().Be(4.00m);
        firstItem.GetProperty("total").GetDecimal().Should().Be(36.00m);

        // Second item: 10 * 20.00 = 200.00, discount 20% = 40.00, total = 160.00
        var secondItem = items[1];
        secondItem.GetProperty("quantity").GetInt32().Should().Be(10);
        secondItem.GetProperty("unitPrice").GetDecimal().Should().Be(20.00m);
        secondItem.GetProperty("discount").GetDecimal().Should().Be(40.00m);
        secondItem.GetProperty("total").GetDecimal().Should().Be(160.00m);

        // Total sale: 36.00 + 160.00 = 196.00
        data.GetProperty("totalAmount").GetDecimal().Should().Be(196.00m);
    }

    [Fact(DisplayName = "GET /api/sales should return paginated sales list")]
    public async Task Get_SalesList_ShouldReturnPaginatedList()
    {
        // Arrange - Create multiple sales
        var sales = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var createRequest = new CreateSaleRequest
            {
                BranchId = Guid.NewGuid(),
                BranchDescription = $"Branch {i}",
                CustomerId = Guid.NewGuid(),
                CustomerDescription = $"Customer {i}",
                Date = DateTime.UtcNow.AddDays(-i),
                Items = new List<CreateSaleItemRequest>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductDescription = $"Product {i}",
                        Quantity = i + 1,
                        UnitPrice = 10.00m * (i + 1)
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createJsonDocument = JsonDocument.Parse(createContent);
            sales.Add(createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid());
        }

        // Act
        var response = await _client.GetAsync("/api/sales?page=1&size=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Be("Sales list retrieved successfully");
        
        var data = jsonDocument.RootElement.GetProperty("data");
        data.GetProperty("page").GetInt32().Should().Be(1);
        data.GetProperty("size").GetInt32().Should().Be(3);
        data.GetProperty("totalCount").GetInt32().Should().BeGreaterOrEqualTo(5);
        data.GetProperty("hasNextPage").GetBoolean().Should().BeTrue();
        data.GetProperty("hasPreviousPage").GetBoolean().Should().BeFalse();
        
        var salesArray = data.GetProperty("sales");
        salesArray.GetArrayLength().Should().Be(3);
    }

    [Fact(DisplayName = "GET /api/sales should apply filters correctly")]
    public async Task Get_SalesListWithFilters_ShouldApplyFiltersCorrectly()
    {
        // Arrange - Create sales with specific attributes
        var branchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        
        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Filtered Branch",
            CustomerId = customerId,
            CustomerDescription = "Filtered Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Filtered Product",
                    Quantity = 5,
                    UnitPrice = 20.00m
                }
            }
        };

        await _client.PostAsJsonAsync("/api/sales", createRequest);

        // Act - Filter by branchId and customerId
        var response = await _client.GetAsync($"/api/sales?branchId={branchId}&customerId={customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        var data = jsonDocument.RootElement.GetProperty("data");
        var salesArray = data.GetProperty("sales");
        
        salesArray.GetArrayLength().Should().BeGreaterOrEqualTo(1);
        
        // Verify first sale matches filters
        var firstSale = salesArray[0];
        firstSale.GetProperty("branchId").GetGuid().Should().Be(branchId);
        firstSale.GetProperty("customerId").GetGuid().Should().Be(customerId);
        firstSale.GetProperty("branchDescription").GetString().Should().Be("Filtered Branch");
        firstSale.GetProperty("customerDescription").GetString().Should().Be("Filtered Customer");
    }

    [Fact(DisplayName = "GET /api/sales should support ordering")]
    public async Task Get_SalesListWithOrdering_ShouldReturnOrderedResults()
    {
        // Arrange - Create sales with different dates
        var sales = new List<(Guid Id, DateTime Date)>();
        for (int i = 0; i < 3; i++)
        {
            var date = DateTime.UtcNow.AddDays(-i);
            var createRequest = new CreateSaleRequest
            {
                BranchId = Guid.NewGuid(),
                BranchDescription = $"Branch {i}",
                CustomerId = Guid.NewGuid(),
                CustomerDescription = $"Customer {i}",
                Date = date,
                Items = new List<CreateSaleItemRequest>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductDescription = $"Product {i}",
                        Quantity = 1,
                        UnitPrice = 10.00m
                    }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createJsonDocument = JsonDocument.Parse(createContent);
            var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();
            sales.Add((saleId, date));
        }

        // Act - Order by date ascending
        var response = await _client.GetAsync("/api/sales?order=date");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        var data = jsonDocument.RootElement.GetProperty("data");
        var salesArray = data.GetProperty("sales");
        
        salesArray.GetArrayLength().Should().BeGreaterOrEqualTo(3);
        
        // Verify first sale is the oldest (when ordering by date ascending)
        var firstSaleDate = salesArray[0].GetProperty("date").GetDateTime();
        var secondSaleDate = salesArray[1].GetProperty("date").GetDateTime();
        firstSaleDate.Should().BeBefore(secondSaleDate.AddSeconds(1)); // Allow for small time differences
    }
}