using Microsoft.AspNetCore.Mvc;

namespace DeveloperEvaluation.WebApi.Features.Sales.ListSales;

public class ListSalesRequest
{
    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "size")]
    public int Size { get; set; } = 10;

    [FromQuery(Name = "order")]
    public string? Order { get; set; }

    [FromQuery(Name = "minDate")]
    public DateTime? MinDate { get; set; }

    [FromQuery(Name = "maxDate")]
    public DateTime? MaxDate { get; set; }

    [FromQuery(Name = "customerId")]
    public Guid? CustomerId { get; set; }

    [FromQuery(Name = "branchId")]
    public Guid? BranchId { get; set; }

    [FromQuery(Name = "cancelled")]
    public bool? Cancelled { get; set; }

    [FromQuery(Name = "saleNumber")]
    public string? SaleNumber { get; set; }
}