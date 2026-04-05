using MediatR;

namespace DeveloperEvaluation.Application.Sales.UpdateSale;

public record UpdateSaleCommand(
    Guid Id,
    DateTime Date,
    List<UpdateSaleItemDto> Items
) : IRequest<UpdateSaleResult>;

public record UpdateSaleItemDto(
    Guid? Id,
    Guid ProductId,
    int Quantity
);