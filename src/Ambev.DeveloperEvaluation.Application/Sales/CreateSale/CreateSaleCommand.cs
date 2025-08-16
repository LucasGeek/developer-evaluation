using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Command for creating a new sale
/// </summary>
public record CreateSaleCommand(
    Guid BranchId,
    Guid CustomerId,
    List<CreateSaleItemDto> Items
) : IRequest<Guid>;

/// <summary>
/// DTO for sale items in create command
/// </summary>
public record CreateSaleItemDto(
    Guid ProductId,
    int Quantity
);