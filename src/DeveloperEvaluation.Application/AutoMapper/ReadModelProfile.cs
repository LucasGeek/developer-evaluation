using AutoMapper;
using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.ORM.MongoDB.ReadModels;

namespace DeveloperEvaluation.Application.AutoMapper;

public class ReadModelProfile : Profile
{
    public ReadModelProfile()
    {
        CreateMap<Sale, SaleReadModel>()
            .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => src.Cancelled))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
            
        CreateMap<SaleItem, SaleItemReadModel>()
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Total));

        CreateMap<Product, ProductReadModel>()
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => new ProductRatingReadModel 
            { 
                Rate = src.Rating.Rate, 
                Count = src.Rating.Count 
            }));

        CreateMap<User, UserReadModel>();
    }
}