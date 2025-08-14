using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleItemValidatorTests
{
    private readonly CancelSaleItemValidator _validator = new();

    [Fact(DisplayName = "Valid command should pass validation")]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        var command = new CancelSaleItemCommand
        {
            SaleId = Guid.NewGuid(),
            ItemId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Empty SaleId should fail validation")]
    public void Validate_EmptySaleId_ShouldFailValidation()
    {
        // Arrange
        var command = new CancelSaleItemCommand
        {
            SaleId = Guid.Empty,
            ItemId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("Sale ID is required"));
    }

    [Fact(DisplayName = "Empty ItemId should fail validation")]
    public void Validate_EmptyItemId_ShouldFailValidation()
    {
        // Arrange
        var command = new CancelSaleItemCommand
        {
            SaleId = Guid.NewGuid(),
            ItemId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("Item ID is required"));
    }
}