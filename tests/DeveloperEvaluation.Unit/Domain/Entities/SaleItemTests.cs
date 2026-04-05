using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    [Theory(DisplayName = "Should apply correct discount rates based on quantity")]
    [InlineData(1, 10, 0, 10)]     // quantity 1: no discount
    [InlineData(3, 10, 0, 30)]     // quantity 3: no discount
    [InlineData(4, 10, 4, 36)]     // quantity 4: 10% discount (40 - 4)
    [InlineData(9, 10, 9, 81)]     // quantity 9: 10% discount (90 - 9)
    [InlineData(10, 10, 20, 80)]   // quantity 10: 20% discount (100 - 20)
    [InlineData(20, 10, 40, 160)]  // quantity 20: 20% discount (200 - 40)
    public void Given_ItemWithQuantity_When_ApplyingDiscount_Then_ShouldCalculateCorrectDiscountAndTotal(
        int quantity, decimal unitPrice, decimal expectedDiscount, decimal expectedTotal)
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Test Product", quantity, unitPrice);

        // Act
        item.ApplyDiscount();

        // Assert
        item.Discount.Should().Be(expectedDiscount);
        item.Total.Should().Be(expectedTotal);
    }

    [Fact(DisplayName = "Should throw exception when quantity exceeds 20")]
    public void Given_QuantityOver20_When_ApplyingDiscount_Then_ShouldThrowException()
    {
        // Arrange
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Test Product", 21, 10m);

        // Act & Assert
        var action = () => item.ApplyDiscount();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Quantity cannot exceed 20 per item.");
    }

    [Fact(DisplayName = "Should throw exception when updating quantity over 20")]
    public void Given_QuantityOver20_When_UpdatingQuantityAndPrice_Then_ShouldThrowException()
    {
        // Arrange
        var item = SaleTestData.GenerateValidSaleItem();

        // Act & Assert
        var action = () => item.UpdateQuantityAndPrice(21, 10m);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Quantity cannot exceed 20 per item.");
    }

    [Fact(DisplayName = "Should update quantity and price correctly")]
    public void Given_ValidQuantityAndPrice_When_UpdatingQuantityAndPrice_Then_ShouldUpdateCorrectly()
    {
        // Arrange
        var item = SaleTestData.GenerateValidSaleItem();
        var newQuantity = 5;
        var newUnitPrice = 15m;

        // Act
        item.UpdateQuantityAndPrice(newQuantity, newUnitPrice);

        // Assert
        item.Quantity.Should().Be(newQuantity);
        item.UnitPrice.Should().Be(newUnitPrice);
    }

    [Theory(DisplayName = "Edge cases for discount calculation")]
    [InlineData(3, 0)]    // Just below 10% discount threshold
    [InlineData(4, 0.10)] // Exactly at 10% discount threshold
    [InlineData(9, 0.10)] // Just below 20% discount threshold
    [InlineData(10, 0.20)] // Exactly at 20% discount threshold
    [InlineData(20, 0.20)] // Maximum allowed quantity
    public void Given_EdgeCaseQuantities_When_ApplyingDiscount_Then_ShouldApplyCorrectDiscountRate(
        int quantity, decimal expectedDiscountRate)
    {
        // Arrange
        var unitPrice = 10m;
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Test Product", quantity, unitPrice);

        // Act
        item.ApplyDiscount();

        // Assert
        var expectedDiscount = Math.Round(unitPrice * quantity * expectedDiscountRate, 2);
        item.Discount.Should().Be(expectedDiscount);
    }

    [Fact(DisplayName = "Should create item with valid data")]
    public void Given_ValidData_When_CreatingItem_Then_ShouldCreateSuccessfully()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productDescription = "Test Product";
        var quantity = 5;
        var unitPrice = 10m;

        // Act
        var item = new SaleItem(saleId, productId, productDescription, quantity, unitPrice);

        // Assert
        item.SaleId.Should().Be(saleId);
        item.ProductId.Should().Be(productId);
        item.ProductDescription.Should().Be(productDescription);
        item.Quantity.Should().Be(quantity);
        item.UnitPrice.Should().Be(unitPrice);
        item.Discount.Should().Be(0); // Not applied yet
        item.Total.Should().Be(0); // Not calculated yet
    }
}