using MediatR;

namespace DeveloperEvaluation.Application.Sales.GetSale;

public record GetSaleQuery(Guid Id) : IRequest<GetSaleResult?>;