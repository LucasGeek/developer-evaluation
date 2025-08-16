using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sale ID is required");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Sale date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Sale date cannot be in the future");


        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required")
            .Must(items => items.Count <= 20)
            .WithMessage("Cannot have more than 20 items in a sale");

        RuleForEach(x => x.Items).SetValidator(new UpdateSaleItemValidator());

        RuleFor(x => x.Items)
            .Must(items => items.GroupBy(i => i.ProductId).All(g => g.Sum(i => i.Quantity) <= 20))
            .WithMessage("Cannot have more than 20 identical items in a sale");
    }
}

public class UpdateSaleItemValidator : AbstractValidator<UpdateSaleItemDto>
{
    public UpdateSaleItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(20)
            .WithMessage("Quantity must be between 1 and 20 (business rule)");
    }
}