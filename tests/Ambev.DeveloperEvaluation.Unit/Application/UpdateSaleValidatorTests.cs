using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UpdateSaleValidatorTests
{
    private readonly UpdateSaleValidator _validator = new();

    [Fact(DisplayName = "Valid command should pass validation")]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            CustomerDescription = "Test Customer",
            BranchDescription = "Test Branch",
            Items = new List<UpdateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Test Product",
                    Quantity = 5,
                    UnitPrice = 10.00m
                }
            }
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
        var command = new UpdateSaleCommand
        {
            Id = Guid.Empty,
            Date = DateTime.UtcNow,
            CustomerDescription = "Test Customer",
            BranchDescription = "Test Branch",
            Items = new List<UpdateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Test Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("Sale ID is required"));
    }

    [Fact(DisplayName = "Future date should fail validation")]
    public void Validate_FutureDate_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.AddDays(2), // Future date
            CustomerDescription = "Test Customer",
            BranchDescription = "Test Branch",
            Items = new List<UpdateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Test Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("cannot be in the future"));
    }

    [Fact(DisplayName = "Empty customer description should fail validation")]
    public void Validate_EmptyCustomerDescription_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            CustomerDescription = "",
            BranchDescription = "Test Branch",
            Items = new List<UpdateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Test Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("Customer description is required"));
    }

    [Fact(DisplayName = "Empty items list should fail validation")]
    public void Validate_EmptyItems_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            CustomerDescription = "Test Customer",
            BranchDescription = "Test Branch",
            Items = new List<UpdateSaleItemCommand>() // Empty list
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("At least one item is required"));
    }

    [Fact(DisplayName = "More than 20 items should fail validation")]
    public void Validate_MoreThan20Items_ShouldFailValidation()
    {
        // Arrange
        var items = new List<UpdateSaleItemCommand>();
        for (int i = 0; i < 21; i++)
        {
            items.Add(new UpdateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductDescription = $"Product {i}",
                Quantity = 1,
                UnitPrice = 10.00m
            });
        }

        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            CustomerDescription = "Test Customer",
            BranchDescription = "Test Branch",
            Items = items
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("Cannot have more than 20 items"));
    }

    [Fact(DisplayName = "More than 20 identical items should fail validation")]
    public void Validate_MoreThan20IdenticalItems_ShouldFailValidation()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            CustomerDescription = "Test Customer",
            BranchDescription = "Test Branch",
            Items = new List<UpdateSaleItemCommand>
            {
                new()
                {
                    ProductId = productId,
                    ProductDescription = "Product",
                    Quantity = 15,
                    UnitPrice = 10.00m
                },
                new()
                {
                    ProductId = productId,
                    ProductDescription = "Product",
                    Quantity = 10, // Total = 25 > 20
                    UnitPrice = 10.00m
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("Cannot have more than 20 identical items"));
    }

    [Theory(DisplayName = "Invalid item quantities should fail validation")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public void Validate_InvalidItemQuantity_ShouldFailValidation(int quantity)
    {
        // Arrange
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            CustomerDescription = "Test Customer",
            BranchDescription = "Test Branch",
            Items = new List<UpdateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Test Product",
                    Quantity = quantity,
                    UnitPrice = 10.00m
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Quantity"));
    }

    [Theory(DisplayName = "Invalid unit prices should fail validation")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.5)]
    public void Validate_InvalidUnitPrice_ShouldFailValidation(decimal unitPrice)
    {
        // Arrange
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            CustomerDescription = "Test Customer",
            BranchDescription = "Test Branch",
            Items = new List<UpdateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Test Product",
                    Quantity = 1,
                    UnitPrice = unitPrice
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Unit price must be greater than 0"));
    }
}