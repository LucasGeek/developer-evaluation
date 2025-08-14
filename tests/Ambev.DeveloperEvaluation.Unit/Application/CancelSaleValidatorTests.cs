using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleValidatorTests
{
    private readonly CancelSaleValidator _validator = new();

    [Fact(DisplayName = "Valid command should pass validation")]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        var command = new CancelSaleCommand
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Empty ID should fail validation")]
    public void Validate_EmptyId_ShouldFailValidation()
    {
        // Arrange
        var command = new CancelSaleCommand
        {
            Id = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("Sale ID is required"));
    }
}