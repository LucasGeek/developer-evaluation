using DeveloperEvaluation.Application.Sales.ListSales;
using DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace DeveloperEvaluation.WebApi.Features.Sales.ListSales;

public class ListSalesProfile : Profile
{
    public ListSalesProfile()
    {
        CreateMap<ListSalesRequest, ListSalesQuery>();
        CreateMap<ListSalesResult, ListSalesResponse>();
        CreateMap<ListSaleItemResult, ListSaleItemResponse>();
        CreateMap<Sale, ListSaleItemResult>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));
    }
}