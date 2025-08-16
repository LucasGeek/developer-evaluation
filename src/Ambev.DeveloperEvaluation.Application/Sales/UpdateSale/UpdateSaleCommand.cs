using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public record UpdateSaleCommand(
    Guid Id,
    DateTime Date,
    List<UpdateSaleItemDto> Items
) : IRequest<UpdateSaleResult>;

public record UpdateSaleItemDto(
    Guid? Id, // null for new items
    Guid ProductId,
    int Quantity
);