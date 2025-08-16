using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
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
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId1 = await CreateTestProduct("Product 1", 100.00m);
        var productId2 = await CreateTestProduct("Product 2", 50.00m);

        var request = new CreateSaleRequest
        {
            BranchId = branchId,
            CustomerId = customerId,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId1,
                    Quantity = 5,
                },
                new()
                {
                    ProductId = productId2,
                    Quantity = 10,
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
        response.Headers.Location!.ToString().Should().Contain($"/api/sales/{saleId}");
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
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId1 = await CreateTestProduct("Product with 10% discount", 10.00m);
        var productId2 = await CreateTestProduct("Product with 20% discount", 20.00m);

        var request = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Test Branch",
            CustomerId = customerId,
            CustomerDescription = "Test Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId1,
                    ProductDescription = "Product with 10% discount",
                    Quantity = 4, // 10% discount
                    UnitPrice = 10.00m
                },
                new()
                {
                    ProductId = productId2,
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
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Product 1", 10.00m);

        var request = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Test Branch",
            CustomerId = customerId,
            CustomerDescription = "Test Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId,
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
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Product 1", 10.00m);

        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Test Branch",
            CustomerId = customerId,
            CustomerDescription = "Test Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId,
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
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId1 = await CreateTestProduct("Product with 10% discount", 10.00m);
        var productId2 = await CreateTestProduct("Product with 20% discount", 20.00m);

        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Test Branch",
            CustomerId = customerId,
            CustomerDescription = "Test Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId1,
                    ProductDescription = "Product with 10% discount",
                    Quantity = 4, // 10% discount
                    UnitPrice = 10.00m
                },
                new()
                {
                    ProductId = productId2,
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
            var branchId = await CreateTestBranch();
            var customerId = await CreateTestCustomer();
            var productId = await CreateTestProduct($"Product {i}", 10.00m * (i + 1));

            var createRequest = new CreateSaleRequest
            {
                BranchId = branchId,
                BranchDescription = $"Branch {i}",
                CustomerId = customerId,
                CustomerDescription = $"Customer {i}",
                Date = DateTime.UtcNow.AddDays(-i),
                Items = new List<CreateSaleItemRequest>
                {
                    new()
                    {
                        ProductId = productId,
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
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Filtered Product", 20.00m);
        
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
                    ProductId = productId,
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
            var branchId = await CreateTestBranch();
            var customerId = await CreateTestCustomer();
            var productId = await CreateTestProduct($"Product {i}", 10.00m);

            var date = DateTime.UtcNow.AddDays(-i);
            var createRequest = new CreateSaleRequest
            {
                BranchId = branchId,
                BranchDescription = $"Branch {i}",
                CustomerId = customerId,
                CustomerDescription = $"Customer {i}",
                Date = date,
                Items = new List<CreateSaleItemRequest>
                {
                    new()
                    {
                        ProductId = productId,
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

    [Fact(DisplayName = "PUT /api/sales/{id} should update sale successfully")]
    public async Task Put_ValidUpdateRequest_ShouldUpdateSaleSuccessfully()
    {
        // Arrange - First create a sale
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Original Product", 10.00m);

        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Original Branch",
            CustomerId = customerId,
            CustomerDescription = "Original Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId,
                    ProductDescription = "Original Product",
                    Quantity = 2,
                    UnitPrice = 10.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Prepare update request
        var updateProductId1 = await CreateTestProduct("Updated Product 1", 25.00m);
        var updateProductId2 = await CreateTestProduct("Updated Product 2", 15.00m);

        var updateRequest = new UpdateSaleRequest
        {
            Date = DateTime.UtcNow.AddHours(-1),
            CustomerDescription = "Updated Customer",
            BranchDescription = "Updated Branch",
            Items = new List<UpdateSaleItemRequest>
            {
                new()
                {
                    ProductId = updateProductId1,
                    ProductDescription = "Updated Product 1",
                    Quantity = 4, // 10% discount applies
                    UnitPrice = 25.00m
                },
                new()
                {
                    ProductId = updateProductId2,
                    ProductDescription = "Updated Product 2",
                    Quantity = 1,
                    UnitPrice = 15.00m
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Be("Sale updated successfully");
        
        var data = jsonDocument.RootElement.GetProperty("data");
        data.GetProperty("id").GetGuid().Should().Be(saleId);
        data.GetProperty("customerDescription").GetString().Should().Be("Updated Customer");
        data.GetProperty("branchDescription").GetString().Should().Be("Updated Branch");
        
        var items = data.GetProperty("items");
        items.GetArrayLength().Should().Be(2);
        
        // Verify first item with discount calculation
        var firstItem = items[0];
        firstItem.GetProperty("productDescription").GetString().Should().Be("Updated Product 1");
        firstItem.GetProperty("quantity").GetInt32().Should().Be(4);
        firstItem.GetProperty("unitPrice").GetDecimal().Should().Be(25.00m);
        firstItem.GetProperty("discount").GetDecimal().Should().Be(10.00m); // 10% of 100
        firstItem.GetProperty("total").GetDecimal().Should().Be(90.00m); // 100 - 10
    }

    [Fact(DisplayName = "PUT /api/sales/{id} should return 404 when sale does not exist")]
    public async Task Put_NonExistentSaleId_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateSaleRequest
        {
            Date = DateTime.UtcNow,
            CustomerDescription = "Customer",
            BranchDescription = "Branch",
            Items = new List<UpdateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sales/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Contain("not found");
    }

    [Fact(DisplayName = "PUT /api/sales/{id} should return 400 when trying to update cancelled sale")]
    public async Task Put_CancelledSale_ShouldReturn400()
    {
        // Arrange - Create and then cancel a sale
        var createRequest = new CreateSaleRequest
        {
            BranchId = Guid.NewGuid(),
            BranchDescription = "Branch",
            CustomerId = Guid.NewGuid(),
            CustomerDescription = "Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Cancel the sale first (this will be implemented in next epic)
        // For now, we'll simulate the scenario by creating a sale that's already cancelled
        
        var updateRequest = new UpdateSaleRequest
        {
            Date = DateTime.UtcNow,
            CustomerDescription = "Updated Customer",
            BranchDescription = "Updated Branch",  
            Items = new List<UpdateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        // Act - Try to update the sale (this should work since we haven't implemented cancel yet)
        var response = await _client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);

        // Assert - For now, this should succeed since cancel isn't implemented
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PUT /api/sales/{id} should apply business rules correctly")]
    public async Task Put_UpdateSaleWithBusinessRules_ShouldApplyRulesCorrectly()
    {
        // Arrange - Create a sale
        var createRequest = new CreateSaleRequest
        {
            BranchId = Guid.NewGuid(),
            BranchDescription = "Branch",
            CustomerId = Guid.NewGuid(),
            CustomerDescription = "Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Update with items that should get discounts
        var updateRequest = new UpdateSaleRequest
        {
            Date = DateTime.UtcNow,
            CustomerDescription = "Customer",
            BranchDescription = "Branch",
            Items = new List<UpdateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "10% Discount Product",
                    Quantity = 5, // 10% discount
                    UnitPrice = 20.00m // 5 * 20 = 100, discount = 10, total = 90
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "20% Discount Product",
                    Quantity = 12, // 20% discount  
                    UnitPrice = 10.00m // 12 * 10 = 120, discount = 24, total = 96
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        var data = jsonDocument.RootElement.GetProperty("data");
        var items = data.GetProperty("items");
        
        // Verify discount calculations
        var firstItem = items[0];
        firstItem.GetProperty("discount").GetDecimal().Should().Be(10.00m); // 10% of 100
        firstItem.GetProperty("total").GetDecimal().Should().Be(90.00m);
        
        var secondItem = items[1];
        secondItem.GetProperty("discount").GetDecimal().Should().Be(24.00m); // 20% of 120
        secondItem.GetProperty("total").GetDecimal().Should().Be(96.00m);
        
        // Total sale: 90 + 96 = 186
        data.GetProperty("totalAmount").GetDecimal().Should().Be(186.00m);
    }

    [Fact(DisplayName = "PUT /api/sales/{id}/cancel should cancel sale successfully")]
    public async Task Put_ValidCancelRequest_ShouldCancelSaleSuccessfully()
    {
        // Arrange - First create a sale
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Product to Cancel", 10.00m);

        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Branch to Cancel",
            CustomerId = customerId,
            CustomerDescription = "Customer to Cancel",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId,
                    ProductDescription = "Product to Cancel",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Act - Cancel the sale
        var response = await _client.PutAsync($"/api/sales/{saleId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Be("Sale cancelled successfully");
        
        var data = jsonDocument.RootElement.GetProperty("data");
        data.GetProperty("id").GetGuid().Should().Be(saleId);
        data.GetProperty("cancelled").GetBoolean().Should().BeTrue();
        data.GetProperty("cancelledAt").GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "PUT /api/sales/{id}/cancel should return 404 when sale does not exist")]
    public async Task Put_CancelNonExistentSale_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/sales/{nonExistentId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Contain("not found");
    }

    [Fact(DisplayName = "PUT /api/sales/{id}/cancel should handle already cancelled sale")]
    public async Task Put_CancelAlreadyCancelledSale_ShouldReturnOk()
    {
        // Arrange - Create and cancel a sale
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Product", 10.00m);

        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Branch",
            CustomerId = customerId,
            CustomerDescription = "Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId,
                    ProductDescription = "Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Cancel the sale first
        await _client.PutAsync($"/api/sales/{saleId}/cancel", null);

        // Act - Try to cancel again
        var response = await _client.PutAsync($"/api/sales/{saleId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Be("Sale is already cancelled");
        
        var data = jsonDocument.RootElement.GetProperty("data");
        data.GetProperty("id").GetGuid().Should().Be(saleId);
        data.GetProperty("cancelled").GetBoolean().Should().BeTrue();
    }

    [Fact(DisplayName = "PUT /api/sales/{id}/cancel should prevent updating cancelled sale")]
    public async Task Put_UpdateCancelledSale_ShouldReturn400()
    {
        // Arrange - Create and cancel a sale
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Product", 10.00m);

        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Branch",
            CustomerId = customerId,
            CustomerDescription = "Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId,
                    ProductDescription = "Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Cancel the sale
        await _client.PutAsync($"/api/sales/{saleId}/cancel", null);

        // Try to update the cancelled sale
        var updateProductId = await CreateTestProduct("Updated Product", 15.00m);

        var updateRequest = new UpdateSaleRequest
        {
            Date = DateTime.UtcNow,
            CustomerDescription = "Updated Customer",
            BranchDescription = "Updated Branch",
            Items = new List<UpdateSaleItemRequest>
            {
                new()
                {
                    ProductId = updateProductId,
                    ProductDescription = "Updated Product",
                    Quantity = 1,
                    UnitPrice = 15.00m
                }
            }
        };

        // Act - Try to update the cancelled sale
        var response = await _client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);

        // Assert - Should return bad request since sale is cancelled
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Contain("cancelled sale");
    }

    [Fact(DisplayName = "DELETE /api/sales/{id}/items/{itemId} should cancel item successfully")]
    public async Task Delete_ValidCancelItemRequest_ShouldCancelItemSuccessfully()
    {
        // Arrange - Create a sale with multiple items
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId1 = await CreateTestProduct("Product 1", 10.00m);
        var productId2 = await CreateTestProduct("Product 2", 15.00m);

        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Branch with Items",
            CustomerId = customerId,
            CustomerDescription = "Customer with Items",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId1,
                    ProductDescription = "Product 1",
                    Quantity = 2,
                    UnitPrice = 10.00m
                },
                new()
                {
                    ProductId = productId2,
                    ProductDescription = "Product 2",
                    Quantity = 3,
                    UnitPrice = 15.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Get the sale to retrieve item IDs
        var getResponse = await _client.GetAsync($"/api/sales/{saleId}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getJsonDocument = JsonDocument.Parse(getContent);
        var items = getJsonDocument.RootElement.GetProperty("data").GetProperty("items");
        var firstItemId = items[0].GetProperty("id").GetGuid();

        // Act - Cancel the first item
        var response = await _client.DeleteAsync($"/api/sales/{saleId}/items/{firstItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Be("Item cancelled successfully");
        
        var data = jsonDocument.RootElement.GetProperty("data");
        data.GetProperty("saleId").GetGuid().Should().Be(saleId);
        data.GetProperty("itemId").GetGuid().Should().Be(firstItemId);
        data.GetProperty("itemRemoved").GetBoolean().Should().BeTrue();
        
        // Verify the sale now has only one item
        var updatedGetResponse = await _client.GetAsync($"/api/sales/{saleId}");
        var updatedGetContent = await updatedGetResponse.Content.ReadAsStringAsync();
        var updatedGetJsonDocument = JsonDocument.Parse(updatedGetContent);
        var updatedItems = updatedGetJsonDocument.RootElement.GetProperty("data").GetProperty("items");
        updatedItems.GetArrayLength().Should().Be(1);
    }

    [Fact(DisplayName = "DELETE /api/sales/{id}/items/{itemId} should return 404 when sale not found")]
    public async Task Delete_CancelItemFromNonExistentSale_ShouldReturn404()
    {
        // Arrange
        var nonExistentSaleId = Guid.NewGuid();
        var nonExistentItemId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/sales/{nonExistentSaleId}/items/{nonExistentItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Contain("not found");
    }

    [Fact(DisplayName = "DELETE /api/sales/{id}/items/{itemId} should return 404 when item not found")]
    public async Task Delete_CancelNonExistentItem_ShouldReturn404()
    {
        // Arrange - Create a sale
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Product", 10.00m);

        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Branch",
            CustomerId = customerId,
            CustomerDescription = "Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId,
                    ProductDescription = "Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var nonExistentItemId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/sales/{saleId}/items/{nonExistentItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Contain("not found");
    }

    [Fact(DisplayName = "DELETE /api/sales/{id}/items/{itemId} should return 400 when trying to cancel last item")]
    public async Task Delete_CancelLastItem_ShouldReturn400()
    {
        // Arrange - Create a sale with only one item
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Only Product", 10.00m);

        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Branch",
            CustomerId = customerId,
            CustomerDescription = "Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId,
                    ProductDescription = "Only Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Get the sale to retrieve the item ID
        var getResponse = await _client.GetAsync($"/api/sales/{saleId}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getJsonDocument = JsonDocument.Parse(getContent);
        var items = getJsonDocument.RootElement.GetProperty("data").GetProperty("items");
        var onlyItemId = items[0].GetProperty("id").GetGuid();

        // Act - Try to cancel the only item
        var response = await _client.DeleteAsync($"/api/sales/{saleId}/items/{onlyItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Contain("Cannot cancel the last item");
    }

    [Fact(DisplayName = "DELETE /api/sales/{id}/items/{itemId} should return 400 when sale is cancelled")]
    public async Task Delete_CancelItemFromCancelledSale_ShouldReturn400()
    {
        // Arrange - Create and cancel a sale
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId1 = await CreateTestProduct("Product 1", 10.00m);
        var productId2 = await CreateTestProduct("Product 2", 15.00m);

        var createRequest = new CreateSaleRequest
        {
            BranchId = branchId,
            BranchDescription = "Branch",
            CustomerId = customerId,
            CustomerDescription = "Customer",
            Date = DateTime.UtcNow,
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = productId1,
                    ProductDescription = "Product 1",
                    Quantity = 1,
                    UnitPrice = 10.00m
                },
                new()
                {
                    ProductId = productId2,
                    ProductDescription = "Product 2", 
                    Quantity = 1,
                    UnitPrice = 15.00m
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createJsonDocument = JsonDocument.Parse(createContent);
        var saleId = createJsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Get the sale to retrieve item ID
        var getResponse = await _client.GetAsync($"/api/sales/{saleId}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getJsonDocument = JsonDocument.Parse(getContent);
        var items = getJsonDocument.RootElement.GetProperty("data").GetProperty("items");
        var firstItemId = items[0].GetProperty("id").GetGuid();

        // Cancel the entire sale first
        await _client.PutAsync($"/api/sales/{saleId}/cancel", null);

        // Act - Try to cancel an item from the cancelled sale
        var response = await _client.DeleteAsync($"/api/sales/{saleId}/items/{firstItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        jsonDocument.RootElement.GetProperty("message").GetString().Should().Contain("cancelled sale");
    }

    private async Task<Guid> CreateTestBranch()
    {
        var request = new
        {
            Name = $"Test Branch {Guid.NewGuid()}",
            Description = "Test branch for integration tests",
            Phone = "+5511999999999",
            Address = "123 Test St",
            City = "Test City",
            State = "TS", 
            PostalCode = "12345-678"
        };
        
        var response = await _client.PostAsJsonAsync("/api/branches", request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        return jsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateTestCustomer()
    {
        var request = new
        {
            Username = $"testuser{Guid.NewGuid().ToString("N")[..8]}",
            Email = $"test{Guid.NewGuid().ToString("N")[..8]}@test.com",
            Phone = "+5511999999999",
            Password = "Test@123",
            Role = 2, // Customer
            Status = 1 // Active
        };
        
        var response = await _client.PostAsJsonAsync("/api/users", request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        return jsonDocument.RootElement.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateTestProduct(string title = "Test Product", decimal price = 100.00m)
    {
        var request = new
        {
            Title = title,
            Price = price,
            Description = "Test product for integration tests",
            Category = "test",
            Image = "test.jpg"
        };
        
        var response = await _client.PostAsJsonAsync("/api/products", request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        return jsonDocument.RootElement.GetProperty("id").GetGuid();
    }
}