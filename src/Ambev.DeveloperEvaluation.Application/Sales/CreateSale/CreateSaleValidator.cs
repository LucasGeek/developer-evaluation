using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty()
            .WithMessage("Branch ID is required");

        RuleFor(x => x.BranchDescription)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Branch description is required and must not exceed 255 characters");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.CustomerDescription)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Customer description is required and must not exceed 255 characters");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Sale date is required");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required")
            .Must(items => items.Count <= 20)
            .WithMessage("Cannot have more than 20 different items in a sale");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateSaleItemValidator());
    }
}

public class CreateSaleItemValidator : AbstractValidator<CreateSaleItemDto>
{
    public CreateSaleItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.ProductDescription)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Product description is required and must not exceed 255 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(20)
            .WithMessage("Quantity must be between 1 and 20");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than zero");
    }
}