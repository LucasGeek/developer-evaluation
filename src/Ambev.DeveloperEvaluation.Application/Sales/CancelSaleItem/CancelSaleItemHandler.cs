using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelSaleItemHandler> _logger;

    public CancelSaleItemHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        ILogger<CancelSaleItemHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling item {ItemId} from sale {SaleId}", request.ItemId, request.SaleId);

        // Get existing sale
        var existingSale = await _saleRepository.GetByIdAsync(request.SaleId);
        if (existingSale == null)
        {
            throw new InvalidOperationException($"Sale with ID {request.SaleId} not found");
        }

        if (existingSale.Cancelled)
        {
            throw new InvalidOperationException("Cannot cancel items from a cancelled sale");
        }

        // Find the item to remove
        var itemToRemove = existingSale.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (itemToRemove == null)
        {
            throw new InvalidOperationException($"Item with ID {request.ItemId} not found in sale {existingSale.SaleNumber}");
        }

        // Check if this is the last item
        if (existingSale.Items.Count == 1)
        {
            throw new InvalidOperationException("Cannot cancel the last item in a sale. Consider cancelling the entire sale instead.");
        }

        _logger.LogInformation("Removing item {ProductDescription} from sale {SaleNumber}", 
            itemToRemove.ProductDescription, existingSale.SaleNumber);

        var productDescription = itemToRemove.ProductDescription;

        // Remove the item using domain method
        existingSale.RemoveItemById(itemToRemove.Id);

        // Update in repository
        await _saleRepository.UpdateAsync(existingSale);

        _logger.LogInformation("Item cancelled successfully from sale {SaleNumber}, New Total: {TotalAmount}", 
            existingSale.SaleNumber, existingSale.TotalAmount);

        // Return result
        var result = new CancelSaleItemResult
        {
            SaleId = existingSale.Id,
            SaleNumber = existingSale.SaleNumber,
            ItemId = request.ItemId,
            ProductDescription = productDescription,
            ItemRemoved = true,
            NewTotalAmount = existingSale.TotalAmount,
            Message = "Item cancelled successfully"
        };

        return result;
    }
}