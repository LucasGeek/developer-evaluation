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
            CustomerId: Guid.NewGuid(),
            Items: new List<CreateSaleItemDto>
            {
                new(Guid.NewGuid(), 5)
            });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Empty branch ID should fail validation")]
    public void Given_EmptyBranchId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var command = new CreateSaleCommand(
            BranchId: Guid.Empty,
            CustomerId: Guid.NewGuid(),
            Items: new List<CreateSaleItemDto>
            {
                new(Guid.NewGuid(), 5)
            });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchId)
            .WithErrorMessage("Branch ID is required");
    }

    [Fact(DisplayName = "Empty customer ID should fail validation")]
    public void Given_EmptyCustomerId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var command = new CreateSaleCommand(
            BranchId: Guid.NewGuid(),
            CustomerId: Guid.Empty,
            Items: new List<CreateSaleItemDto>
            {
                new(Guid.NewGuid(), 5)
            });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("Customer ID is required");
    }

    [Fact(DisplayName = "Empty items list should fail validation")]
    public void Given_EmptyItemsList_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var command = new CreateSaleCommand(
            BranchId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
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
            .Select(i => new CreateSaleItemDto(Guid.NewGuid(), 1))
            .ToList();

        var command = new CreateSaleCommand(
            BranchId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
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
            CustomerId: Guid.NewGuid(),
            Items: new List<CreateSaleItemDto>
            {
                new(Guid.NewGuid(), quantity)
            });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact(DisplayName = "Empty product ID should fail validation")]
    public void Given_EmptyProductId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var command = new CreateSaleCommand(
            BranchId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            Items: new List<CreateSaleItemDto>
            {
                new(Guid.Empty, 5)
            });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].ProductId");
    }
}