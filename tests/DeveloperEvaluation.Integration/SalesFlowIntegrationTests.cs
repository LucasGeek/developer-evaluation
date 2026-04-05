using DeveloperEvaluation.Application.Branches.CreateBranch;
using DeveloperEvaluation.Application.Products.CreateProduct;
using DeveloperEvaluation.Application.Sales.CreateSale;
using DeveloperEvaluation.Application.Sales.GetSale;
using DeveloperEvaluation.Application.Sales.CancelSale;
using DeveloperEvaluation.Application.Users.CreateUser;
using DeveloperEvaluation.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using DeveloperEvaluation.WebApi;
using Xunit;

namespace DeveloperEvaluation.Integration;

/// <summary>
/// Integration tests for the complete Sales flow
/// Tests the complete user journey: Create Branch → Create User → Create Product → Create Sale → Verify → Cancel
/// </summary>
public class SalesFlowIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IMediator _mediator;

    public SalesFlowIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        
        // Create a scope to get the mediator for direct command testing
        var scope = _factory.Services.CreateScope();
        _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Fact(DisplayName = "Complete Sales Flow - Should create branch, user, product, sale and verify all requirements")]
    public async Task CompleteSalesFlow_ShouldCreateAndVerifyAllRequirements()
    {
        // Arrange - Create all required entities first
        
        // 1. Create Branch
        var createBranchCommand = new CreateBranchCommand(
            "Main Branch " + Guid.NewGuid(),
            "Main branch for sales",
            "123 Main Street",
            "São Paulo", 
            "SP",
            "01234-567",
            "(11) 99999-9999"
        );
    var branchId = await _mediator.Send(createBranchCommand);
    branchId.Should().NotBe(Guid.Empty, "Branch should be created successfully");

        // 2. Create User (Customer)
        var uniqueEmail = $"john{Guid.NewGuid().ToString("N")[..8]}@example.com";
        var createUserCommand = new CreateUserCommand(
            "John Doe",
            uniqueEmail,
            "+5511987654321",
            UserRole.Customer,
            UserStatus.Active
        ) { Password = "Test@123" };
    var customerResult = await _mediator.Send(createUserCommand);
    Guid customerId = ExtractUserId(customerResult);
    customerId.Should().NotBe(Guid.Empty, "Customer should be created successfully");

        // 3. Create Products
        var createProduct1Command = new CreateProductCommand(
            "Smartphone Samsung Galaxy",
            999.99m,
            "Latest Samsung smartphone",
            "Electronics",
            "https://example.com/samsung.jpg"
        );
    var product1Result = await _mediator.Send(createProduct1Command);
    Guid product1Id = ExtractProductId(product1Result);
    product1Id.Should().NotBe(Guid.Empty, "Product 1 should be created successfully");

        var createProduct2Command = new CreateProductCommand(
            "iPhone 15 Pro",
            1299.99m,
            "Latest iPhone model",
            "Electronics", 
            "https://example.com/iphone.jpg"
        );
    var product2Result = await _mediator.Send(createProduct2Command);
    Guid product2Id = ExtractProductId(product2Result);
    product2Id.Should().NotBe(Guid.Empty, "Product 2 should be created successfully");

        // Act - Create Sale with business rules validation
        var createSaleCommand = new CreateSaleCommand(
            BranchId: branchId,
            CustomerId: customerId,
            Items: new List<CreateSaleItemDto>
            {
                // 5 Samsung phones - should get 10% discount (4-9 items)
                new(product1Id, 5),
                // 12 iPhones - should get 20% discount (10-20 items) 
                new(product2Id, 12)
            }
        );

    var saleId = await _mediator.Send(createSaleCommand);
    saleId.Should().NotBe(Guid.Empty, "Sale should be created successfully");

        // Assert - Verify Sale contains all required information
        var getSaleQuery = new GetSaleQuery(saleId);
        var saleResult = await _mediator.Send(getSaleQuery);

        saleResult.Should().NotBeNull("Sale should be retrievable");

        // ✅ Verify all REQUIRED FIELDS are present:
        
        // Sale number
        saleResult!.SaleNumber.Should().NotBeNullOrEmpty("Sale should have a sale number");
        
        // Date when the sale was made
        saleResult.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5), "Sale date should be recent");
        
        // Customer
        saleResult.CustomerId.Should().Be(customerId, "Sale should have correct customer ID");
        saleResult.CustomerDescription.Should().NotBeNullOrEmpty("Sale should have customer description");
        
        // Branch where the sale was made
        saleResult.BranchId.Should().Be(branchId, "Sale should have correct branch ID");
        saleResult.BranchDescription.Should().NotBeNullOrEmpty("Sale should have branch description");
        
        // Items, Quantities, Unit prices, Discounts, Total amount for each item
        saleResult.Items.Should().HaveCount(2, "Sale should have 2 items");
        
        var samsungItem = saleResult.Items.First(i => i.ProductId == product1Id);
        samsungItem.ProductDescription.Should().Be("Smartphone Samsung Galaxy");
        samsungItem.Quantity.Should().Be(5);
        samsungItem.UnitPrice.Should().Be(999.99m);
        samsungItem.Discount.Should().BeGreaterThan(0, "Samsung item should have 10% discount");
        samsungItem.Total.Should().Be((999.99m * 5) - samsungItem.Discount);
        
        var iphoneItem = saleResult.Items.First(i => i.ProductId == product2Id);
        iphoneItem.ProductDescription.Should().Be("iPhone 15 Pro");
        iphoneItem.Quantity.Should().Be(12);
        iphoneItem.UnitPrice.Should().Be(1299.99m);
        iphoneItem.Discount.Should().BeGreaterThan(0, "iPhone item should have 20% discount");
        iphoneItem.Total.Should().Be((1299.99m * 12) - iphoneItem.Discount);
        
        // Total sale amount
        var expectedTotal = samsungItem.Total + iphoneItem.Total;
        saleResult.TotalAmount.Should().Be(expectedTotal, "Total amount should be sum of item totals");
        
        // Cancelled/Not Cancelled
        saleResult.Cancelled.Should().BeFalse("Sale should not be cancelled initially");
        saleResult.CancelledAt.Should().BeNull("Sale should not have cancellation date initially");

        // ✅ Verify Business Rules Applied Correctly:
        
        // Samsung (5 items): 10% discount  
        var expectedSamsungDiscount = Math.Round(999.99m * 5 * 0.10m, 2);
        samsungItem.Discount.Should().Be(expectedSamsungDiscount, "Samsung should have 10% discount for 5 items");
        
        // iPhone (12 items): 20% discount
        var expectedIphoneDiscount = Math.Round(1299.99m * 12 * 0.20m, 2);
        iphoneItem.Discount.Should().Be(expectedIphoneDiscount, "iPhone should have 20% discount for 12 items");

        // Act - Test Sale Cancellation
        var cancelSaleCommand = new CancelSaleCommand { Id = saleId };
        var cancelResult = await _mediator.Send(cancelSaleCommand);

        cancelResult.Should().NotBeNull("Cancel operation should return result");
        cancelResult.Cancelled.Should().BeTrue("Sale should be marked as cancelled");
        cancelResult.CancelledAt.Should().NotBeNull("Sale should have cancellation timestamp");

        // Verify sale is actually cancelled
        var cancelledSaleResult = await _mediator.Send(getSaleQuery);
        cancelledSaleResult!.Cancelled.Should().BeTrue("Sale should be cancelled in database");
        cancelledSaleResult.CancelledAt.Should().NotBeNull("Cancelled sale should have timestamp");
    }

    [Theory(DisplayName = "Business Rules Validation - Should enforce quantity limits and discount tiers")]
    [InlineData(1, 0)] // No discount for 1 item
    [InlineData(3, 0)] // No discount for 3 items 
    [InlineData(4, 10)] // 10% discount for 4 items
    [InlineData(9, 10)] // 10% discount for 9 items
    [InlineData(10, 20)] // 20% discount for 10 items
    [InlineData(20, 20)] // 20% discount for 20 items
    public async Task BusinessRules_ShouldApplyCorrectDiscountBasedOnQuantity(int quantity, int expectedDiscountPercent)
    {
        // Arrange
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Test Product", 100.00m);

        // Act
        var createSaleCommand = new CreateSaleCommand(
            BranchId: branchId,
            CustomerId: customerId,
            Items: new List<CreateSaleItemDto> { new(productId, quantity) }
        );

        var saleId = await _mediator.Send(createSaleCommand);
        var saleResult = await _mediator.Send(new GetSaleQuery(saleId));

        // Assert
        var item = saleResult!.Items.First();
        var expectedDiscount = expectedDiscountPercent > 0 
            ? Math.Round(100.00m * quantity * (expectedDiscountPercent / 100.0m), 2)
            : 0;

        item.Discount.Should().Be(expectedDiscount, 
            $"Quantity {quantity} should have {expectedDiscountPercent}% discount");
        item.Total.Should().Be((100.00m * quantity) - expectedDiscount);
    }

    [Fact(DisplayName = "Business Rules Validation - Should reject sales with more than 20 items")]
    public async Task BusinessRules_ShouldRejectSalesWithMoreThan20Items()
    {
        // Arrange
        var branchId = await CreateTestBranch();
        var customerId = await CreateTestCustomer();
        var productId = await CreateTestProduct("Test Product", 100.00m);

        // Act & Assert
        var createSaleCommand = new CreateSaleCommand(
            BranchId: branchId,
            CustomerId: customerId,
            Items: new List<CreateSaleItemDto> { new(productId, 21) } // Over limit
        );

        Func<Task> act = async () => await _mediator.Send(createSaleCommand);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*20 identical items*", "Should prevent sales over 20 items limit");
    }

    // Helper methods for test setup
    async Task<Guid> CreateTestBranch()
    {
        var command = new CreateBranchCommand("Test Branch " + Guid.NewGuid().ToString(), "Test", "123 Test St", "Test City", "TS", "12345", "(11) 99999-9999");
    return await _mediator.Send(command);
    }

    async Task<Guid> CreateTestCustomer()
    {
    var uniqueEmail = $"test{Guid.NewGuid().ToString("N")[..8]}@test.com";
    var command = new CreateUserCommand("Test Customer", uniqueEmail, "+5511999999999", UserRole.Customer, UserStatus.Active) { Password = "Test@123" };
    var result = await _mediator.Send(command);
    return ExtractUserId(result);
    }

    async Task<Guid> CreateTestProduct(string title, decimal price)
    {
        var command = new CreateProductCommand(title, price, "Test product", "Test", "test.jpg");
        var result = await _mediator.Send(command);
        return ExtractProductId(result);
    }

    private Guid ExtractUserId(object result)
    {
        if (result is Guid guid)
            return guid;
        if (result is CreateUserResult userResult)
            return userResult.Id;
        throw new InvalidOperationException("Unexpected result type for CreateUserCommand");
    }

    private Guid ExtractProductId(object result)
    {
        if (result is Guid guid)
            return guid;
        if (result is CreateProductResult productResult)
            return productResult.Id;
        throw new InvalidOperationException("Unexpected result type for CreateProductCommand");
    }
}