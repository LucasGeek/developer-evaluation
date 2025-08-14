using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, GetProductResult>()
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => new RatingResult 
            { 
                Rate = src.Rating.Rate, 
                Count = src.Rating.Count 
            }));

        CreateMap<GetProductResult, ProductResponse>()
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => new RatingResponse
            {
                Rate = src.Rating.Rate,
                Count = src.Rating.Count
            }));

        CreateMap<Rating, RatingResult>();
        CreateMap<RatingResult, RatingResponse>();
    }
}