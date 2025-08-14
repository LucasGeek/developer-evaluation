using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public record CreateSaleCommand(
    Guid BranchId,
    string BranchDescription,
    Guid CustomerId,
    string CustomerDescription,
    DateTime Date,
    List<CreateSaleItemDto> Items
) : IRequest<Guid>;

public record CreateSaleItemDto(
    Guid ProductId,
    string ProductDescription,
    int Quantity,
    decimal UnitPrice
);