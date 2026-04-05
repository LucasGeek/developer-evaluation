using MediatR;

namespace DeveloperEvaluation.Domain.Events;

public class SaleCreatedEvent : INotification
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerDescription { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchDescription { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

public class SaleModifiedEvent : INotification
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public decimal PreviousTotalAmount { get; set; }
    public decimal NewTotalAmount { get; set; }
    public int PreviousItemCount { get; set; }
    public int NewItemCount { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModificationReason { get; set; } = string.Empty;
}

public class SaleCancelledEvent : INotification
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CancelledAt { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class ItemCancelledEvent : INotification
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal NewSaleTotal { get; set; }
    public DateTime CancelledAt { get; set; }
}