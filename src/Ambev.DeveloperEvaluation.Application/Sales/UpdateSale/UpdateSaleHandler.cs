using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateSaleHandler> _logger;

    public UpdateSaleHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        ILogger<UpdateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating sale with ID: {SaleId}", request.Id);

        // Get existing sale
        var existingSale = await _saleRepository.GetByIdAsync(request.Id);
        if (existingSale == null)
        {
            throw new InvalidOperationException($"Sale with ID {request.Id} not found");
        }

        if (existingSale.Cancelled)
        {
            throw new InvalidOperationException("Cannot update a cancelled sale");
        }

        _logger.LogInformation("Found existing sale: {SaleNumber}", existingSale.SaleNumber);

        // Update sale properties using domain methods
        existingSale.UpdateSaleDetails(request.Date, request.CustomerDescription, request.BranchDescription);

        // Clear existing items and add new ones
        existingSale.ClearItems();

        foreach (var itemRequest in request.Items)
        {
            var saleItem = new SaleItem(
                saleId: existingSale.Id,
                productId: itemRequest.ProductId,
                productDescription: itemRequest.ProductDescription,
                quantity: itemRequest.Quantity,
                unitPrice: itemRequest.UnitPrice);

            existingSale.AddItem(saleItem);
        }

        // Recalculate totals with business rules
        existingSale.RecalculateTotal();

        // Update in repository
        await _saleRepository.UpdateAsync(existingSale);

        _logger.LogInformation("Sale updated successfully: {SaleNumber}, New Total: {TotalAmount}", 
            existingSale.SaleNumber, existingSale.TotalAmount);

        // Map to result
        var result = _mapper.Map<UpdateSaleResult>(existingSale);
        return result;
    }
}