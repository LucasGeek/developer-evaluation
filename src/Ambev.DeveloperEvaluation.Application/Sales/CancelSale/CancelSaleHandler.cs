using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelSaleHandler> _logger;

    public CancelSaleHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        ILogger<CancelSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CancelSaleResult> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling sale with ID: {SaleId}", request.Id);

        // Get existing sale
        var existingSale = await _saleRepository.GetByIdAsync(request.Id);
        if (existingSale == null)
        {
            throw new InvalidOperationException($"Sale with ID {request.Id} not found");
        }

        if (existingSale.Cancelled)
        {
            _logger.LogWarning("Sale {SaleNumber} is already cancelled", existingSale.SaleNumber);
            return new CancelSaleResult
            {
                Id = existingSale.Id,
                SaleNumber = existingSale.SaleNumber,
                Cancelled = true,
                CancelledAt = existingSale.CancelledAt,
                Message = "Sale is already cancelled"
            };
        }

        _logger.LogInformation("Cancelling sale: {SaleNumber}", existingSale.SaleNumber);

        // Cancel the sale using domain method
        existingSale.Cancel();

        // Update in repository
        await _saleRepository.UpdateAsync(existingSale);

        _logger.LogInformation("Sale cancelled successfully: {SaleNumber} at {CancelledAt}", 
            existingSale.SaleNumber, existingSale.CancelledAt);

        // Map to result
        var result = new CancelSaleResult
        {
            Id = existingSale.Id,
            SaleNumber = existingSale.SaleNumber,
            Cancelled = existingSale.Cancelled,
            CancelledAt = existingSale.CancelledAt,
            Message = "Sale cancelled successfully"
        };

        return result;
    }
}