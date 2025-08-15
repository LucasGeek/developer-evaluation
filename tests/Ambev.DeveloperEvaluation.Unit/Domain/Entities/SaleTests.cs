using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "Sale should be created with valid data")]
    public void Given_ValidData_When_CreatingSale_Then_ShouldCreateSuccessfully()
    {
        // Arrange
        var saleNumber = "BRANCH001-0001";
        var date = DateTime.UtcNow;
        var customerId = Guid.NewGuid();
        var customerDescription = "Test Customer";
        var branchId = Guid.NewGuid();
        var branchDescription = "Test Branch";

        // Act
        var sale = new Sale(saleNumber, date, customerId, customerDescription, branchId, branchDescription);

        // Assert
        sale.SaleNumber.Should().Be(saleNumber);
        sale.Date.Should().Be(date);
        sale.CustomerId.Should().Be(customerId);
        sale.CustomerDescription.Should().Be(customerDescription);
        sale.BranchId.Should().Be(branchId);
        sale.BranchDescription.Should().Be(branchDescription);
        sale.TotalAmount.Should().Be(0);
        sale.Cancelled.Should().BeFalse();
        sale.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Adding item should update total amount")]
    public void Given_SaleItem_When_AddingToSale_Then_ShouldUpdateTotalAmount()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = SaleTestData.GenerateValidSaleItem(sale.Id);

        // Act
        sale.AddItem(item);

        // Assert
        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().BeGreaterThan(0);
        sale.TotalAmount.Should().Be(item.Total);
    }

    [Theory(DisplayName = "Should apply correct discount based on quantity")]
    [InlineData(3, 0)]    // No discount for quantity < 4
    [InlineData(4, 0.10)] // 10% discount for quantity 4-9
    [InlineData(9, 0.10)] // 10% discount for quantity 4-9
    [InlineData(10, 0.20)] // 20% discount for quantity 10-20
    [InlineData(20, 0.20)] // 20% discount for quantity 10-20
    public void Given_ItemWithQuantity_When_AddingToSale_Then_ShouldApplyCorrectDiscount(int quantity, decimal expectedDiscountRate)
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var unitPrice = 10m;
        var item = SaleTestData.GenerateItemWithQuantity(quantity, sale.Id);
        item.UpdateQuantityAndPrice(quantity, unitPrice);

        // Act
        sale.AddItem(item);

        // Assert
        var expectedDiscount = Math.Round(unitPrice * quantity * expectedDiscountRate, 2);
        var expectedTotal = Math.Round((unitPrice * quantity) - expectedDiscount, 2);
        
        item.Discount.Should().Be(expectedDiscount);
        item.Total.Should().Be(expectedTotal);
        sale.TotalAmount.Should().Be(expectedTotal);
    }

    [Fact(DisplayName = "Should throw exception when adding more than 20 items of same product")]
    public void Given_MoreThan20ItemsOfSameProduct_When_AddingToSale_Then_ShouldThrowException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var productId = Guid.NewGuid();
        
        // Add 20 items of the same product
        for (int i = 0; i < 20; i++)
        {
            var item = new SaleItem(sale.Id, productId, "Test Product", 1, 10m);
            sale.AddItem(item);
        }

        // Act & Assert
        var extraItem = new SaleItem(sale.Id, productId, "Test Product", 1, 10m);
        var action = () => sale.AddItem(extraItem);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot exceed 20 items of same product.");
    }

    [Fact(DisplayName = "Should throw exception when item quantity exceeds 20")]
    public void Given_ItemQuantityOver20_When_CreatingItem_Then_ShouldThrowException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = new SaleItem(sale.Id, Guid.NewGuid(), "Test Product", 21, 10m);

        // Act & Assert
        var action = () => sale.AddItem(item);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Quantity cannot exceed 20 per item.");
    }

    [Fact(DisplayName = "Should throw exception when total quantity of same product exceeds 20")]
    public void Given_MultipleItemsSameProduct_When_TotalQuantityExceeds20_Then_ShouldThrowException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var productId = Guid.NewGuid();
        var firstItem = new SaleItem(sale.Id, productId, "Test Product", 15, 10m);
        var secondItem = new SaleItem(sale.Id, productId, "Test Product", 10, 10m); // Total would be 25

        // Act - Add first item successfully
        sale.AddItem(firstItem);
        
        // Act & Assert - Second item should fail
        var action = () => sale.AddItem(secondItem);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot exceed 20 items of same product.");
    }

    [Fact(DisplayName = "Cancel should set cancelled status and timestamp")]
    public void Given_Sale_When_Cancelling_Then_ShouldSetCancelledStatusAndTimestamp()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var beforeCancel = DateTime.UtcNow;

        // Act
        sale.Cancel();

        // Assert
        sale.Cancelled.Should().BeTrue();
        sale.CancelledAt.Should().NotBeNull();
        sale.CancelledAt.Should().BeOnOrAfter(beforeCancel);
    }

    [Fact(DisplayName = "RemoveItem should update total amount")]
    public void Given_SaleWithItems_When_RemovingItem_Then_ShouldUpdateTotalAmount()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithItems(2);
        var itemToRemove = sale.Items.First();
        var originalTotal = sale.TotalAmount;

        // Act
        sale.RemoveItem(itemToRemove.ProductId);

        // Assert
        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().BeLessThan(originalTotal);
    }

    [Fact(DisplayName = "RecalculateTotal should sum all item totals")]
    public void Given_SaleWithItems_When_RecalculatingTotal_Then_ShouldSumAllItemTotals()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithItems(3);
        var expectedTotal = Math.Round(sale.Items.Sum(i => i.Total), 2);

        // Act
        sale.RecalculateTotal();

        // Assert
        sale.TotalAmount.Should().Be(expectedTotal);
    }
}