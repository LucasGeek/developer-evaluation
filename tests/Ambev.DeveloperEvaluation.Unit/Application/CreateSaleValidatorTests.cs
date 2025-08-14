using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleValidatorTests
{
    private readonly CreateSaleValidator _validator;

    public CreateSaleValidatorTests()
    {
        _validator = new CreateSaleValidator();
    }

    [Fact(DisplayName = "Valid command should pass validation")]
    public void Given_ValidCommand_When_Validated_Then_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateSaleCommand(
            BranchId: Guid.NewGuid(),
            BranchDescription: "Test Branch",
            CustomerId: Guid.NewGuid(),
            CustomerDescription: "Test Customer",
            Date: DateTime.UtcNow,
            Items: new List<CreateSaleItemDto>
            {
                new(Guid.NewGuid(), "Product 1", 5, 10.00m)
            });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Empty BranchId should fail validation")]
    public void Given_EmptyBranchId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var command = new CreateSaleCommand(
            BranchId: Guid.Empty,
            BranchDescription: "Test Branch",
            CustomerId: Guid.NewGuid(),
            CustomerDescription: "Test Customer",
            Date: DateTime.UtcNow,
            Items: new List<CreateSaleItemDto>
            {
                new(Guid.NewGuid(), "Product 1", 5, 10.00m)
            });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchId)
            .WithErrorMessage("Branch ID is required");
    }

    [Fact(DisplayName = "Empty items list should fail validation")]
    public void Given_EmptyItemsList_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var command = new CreateSaleCommand(
            BranchId: Guid.NewGuid(),
            BranchDescription: "Test Branch",
            CustomerId: Guid.NewGuid(),
            CustomerDescription: "Test Customer",
            Date: DateTime.UtcNow,
            Items: new List<CreateSaleItemDto>());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("At least one item is required");
    }

    [Fact(DisplayName = "More than 20 items should fail validation")]
    public void Given_MoreThan20Items_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var items = Enumerable.Range(1, 21)
            .Select(i => new CreateSaleItemDto(Guid.NewGuid(), $"Product {i}", 1, 10.00m))
            .ToList();

        var command = new CreateSaleCommand(
            BranchId: Guid.NewGuid(),
            BranchDescription: "Test Branch",
            CustomerId: Guid.NewGuid(),
            CustomerDescription: "Test Customer",
            Date: DateTime.UtcNow,
            Items: items);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Cannot have more than 20 different items in a sale");
    }

    [Theory(DisplayName = "Invalid item quantities should fail validation")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public void Given_InvalidItemQuantity_When_Validated_Then_ShouldHaveError(int quantity)
    {
        // Arrange
        var command = new CreateSaleCommand(
            BranchId: Guid.NewGuid(),
            BranchDescription: "Test Branch",
            CustomerId: Guid.NewGuid(),
            CustomerDescription: "Test Customer",
            Date: DateTime.UtcNow,
            Items: new List<CreateSaleItemDto>
            {
                new(Guid.NewGuid(), "Product 1", quantity, 10.00m)
            });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Theory(DisplayName = "Invalid unit prices should fail validation")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.50)]
    public void Given_InvalidUnitPrice_When_Validated_Then_ShouldHaveError(decimal unitPrice)
    {
        // Arrange
        var command = new CreateSaleCommand(
            BranchId: Guid.NewGuid(),
            BranchDescription: "Test Branch",
            CustomerId: Guid.NewGuid(),
            CustomerDescription: "Test Customer",
            Date: DateTime.UtcNow,
            Items: new List<CreateSaleItemDto>
            {
                new(Guid.NewGuid(), "Product 1", 5, unitPrice)
            });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }

    [Fact(DisplayName = "Long descriptions should fail validation")]
    public void Given_LongDescriptions_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var longDescription = new string('a', 256);
        var command = new CreateSaleCommand(
            BranchId: Guid.NewGuid(),
            BranchDescription: longDescription,
            CustomerId: Guid.NewGuid(),
            CustomerDescription: longDescription,
            Date: DateTime.UtcNow,
            Items: new List<CreateSaleItemDto>
            {
                new(Guid.NewGuid(), longDescription, 5, 10.00m)
            });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchDescription);
        result.ShouldHaveValidationErrorFor(x => x.CustomerDescription);
        result.ShouldHaveValidationErrorFor("Items[0].ProductDescription");
    }
}