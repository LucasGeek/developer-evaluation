using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEventBus _eventBus;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateSaleHandler> _logger;

    public UpdateSaleHandler(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IEventBus eventBus,
        IMapper mapper,
        ILogger<UpdateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _eventBus = eventBus;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating sale with ID: {SaleId}", request.Id);

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

        existingSale.UpdateSaleDetails(request.Date, existingSale.CustomerDescription, existingSale.BranchDescription);

        existingSale.ClearItems();

        foreach (var itemDto in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");
            }

            if (itemDto.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0");
            }

            if (itemDto.Quantity > 20)
            {
                throw new ArgumentException("Cannot sell more than 20 identical items");
            }

            var item = new SaleItem(
                existingSale.Id,
                itemDto.ProductId,
                product.Title,
                itemDto.Quantity,
                product.Price);
            item.ApplyDiscount();
            
            if (item.Discount > 0)
            {
                var discountPercentage = itemDto.Quantity >= 10 ? 20 : 10;
                _logger.LogInformation(
                    "Applied {DiscountPercentage}% discount to product {ProductId} for quantity {Quantity}. Discount amount: {DiscountAmount}",
                    discountPercentage, itemDto.ProductId, itemDto.Quantity, item.Discount);
            }

            existingSale.AddItem(item);
        }

        await _saleRepository.UpdateAsync(existingSale);

        var saleModifiedEvent = new SaleModifiedEvent
        {
            SaleId = existingSale.Id,
            SaleNumber = existingSale.SaleNumber,
            PreviousTotalAmount = 0,
            NewTotalAmount = existingSale.TotalAmount,
            PreviousItemCount = 0,
            NewItemCount = existingSale.Items.Count,
            ModifiedAt = DateTime.UtcNow,
            ModificationReason = "Sale items updated"
        };

        await _eventBus.PublishAsync(saleModifiedEvent);

        _logger.LogInformation("Sale updated successfully: {SaleNumber}, New Total: {TotalAmount}", 
            existingSale.SaleNumber, existingSale.TotalAmount);

        var result = _mapper.Map<UpdateSaleResult>(existingSale);
        return result;
    }
}