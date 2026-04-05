using DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ListSalesHandler> _logger;

    public ListSalesHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        ILogger<ListSalesHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ListSalesResult> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing sales with filters: Page={Page}, Size={Size}, Order={Order}", 
            request.Page, request.Size, request.Order);

        var (sales, totalCount) = await _saleRepository.ListAsync(
            page: request.Page,
            size: request.Size,
            order: request.Order,
            minDate: request.MinDate,
            maxDate: request.MaxDate,
            customerId: request.CustomerId,
            branchId: request.BranchId,
            cancelled: request.Cancelled,
            saleNumber: request.SaleNumber);

        var saleItems = _mapper.Map<List<ListSaleItemResult>>(sales);

        var totalPages = (int)Math.Ceiling((double)totalCount / request.Size);
        
        var result = new ListSalesResult
        {
            Sales = saleItems,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };

        _logger.LogInformation("Found {Count} sales out of {Total} total", 
            sales.Count, totalCount);

        return result;
    }
}