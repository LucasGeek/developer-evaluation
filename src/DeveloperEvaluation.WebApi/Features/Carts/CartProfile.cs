using DeveloperEvaluation.Application.Carts.CreateCart;
using DeveloperEvaluation.Application.Carts.GetCart;
using DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace DeveloperEvaluation.WebApi.Features.Carts;

public class CartProfile : Profile
{
    public CartProfile()
    {
        // Domain to Application
        CreateMap<Cart, GetCartResult>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
        CreateMap<CartItem, CartItemResult>();

        // Application to Web
        CreateMap<GetCartResult, CartResponse>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
        CreateMap<CartItemResult, CartItemResponse>();

        // Web to Application
        CreateMap<CreateCartRequest, CreateCartCommand>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
        CreateMap<CartItemRequest, Application.Carts.CreateCart.CartItemRequest>();
    }
}