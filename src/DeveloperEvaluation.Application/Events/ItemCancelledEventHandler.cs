using MediatR;
using DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Events;

/// <summary>
/// Event handler for ItemCancelledEvent
/// </summary>
public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing ItemCancelledEvent for Item {ItemId} in Sale {SaleNumber} (ID: {SaleId}). " +
                "Product: {ProductDescription}, Quantity: {Quantity}, ItemTotal: {ItemTotal}, " +
                "NewSaleTotal: {NewSaleTotal}, CancelledAt: {CancelledAt}",
                notification.ItemId,
                notification.SaleNumber,
                notification.SaleId,
                notification.ProductDescription,
                notification.Quantity,
                notification.ItemTotal,
                notification.NewSaleTotal,
                notification.CancelledAt);


            _logger.LogInformation(
                "Item cancellation event processed successfully for Item {ItemId} in Sale {SaleNumber}", 
                notification.ItemId, 
                notification.SaleNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error processing ItemCancelledEvent for Item {ItemId} in Sale ID: {SaleId}", 
                notification.ItemId, 
                notification.SaleId);
        }

        await Task.CompletedTask;
    }
}