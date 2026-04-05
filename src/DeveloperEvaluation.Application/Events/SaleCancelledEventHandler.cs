using MediatR;
using DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Events;

/// <summary>
/// Event handler for SaleCancelledEvent
/// </summary>
public class SaleCancelledEventHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing SaleCancelledEvent for Sale {SaleNumber} (ID: {SaleId}). " +
                "TotalAmount: {TotalAmount}, ItemCount: {ItemCount}, CancelledAt: {CancelledAt}",
                notification.SaleNumber,
                notification.SaleId,
                notification.TotalAmount,
                notification.ItemCount,
                notification.CancelledAt);

            _logger.LogInformation("Sale cancellation event processed successfully for Sale {SaleNumber}", notification.SaleNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SaleCancelledEvent for Sale ID: {SaleId}", notification.SaleId);
        }

        await Task.CompletedTask;
    }
}