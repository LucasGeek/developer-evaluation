using AutoMapper;

namespace DeveloperEvaluation.WebApi.Features.Users.GetUser;

/// <summary>
/// Profile for mapping GetUser feature requests to commands
/// </summary>
public class GetUserProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetUser feature
    /// </summary>
    public GetUserProfile()
    {
        CreateMap<Guid, Application.Users.GetUser.GetUserCommand>()
            .ConstructUsing(id => new Application.Users.GetUser.GetUserCommand(id));

        CreateMap<Application.Users.GetUser.GetUserResult, GetUserResponse>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom((src, _, _, _) =>
            {
                var parts = src.Username.Split(new char[]{' '}, 2);
                return new UserName { Firstname = parts[0], Lastname = parts.Length > 1 ? parts[1] : "" };
            }))
            .ForMember(dest => dest.Password, opt => opt.MapFrom((_, _, _, _) => "*****"))
            .ForMember(dest => dest.Address, opt => opt.MapFrom((_, _, _, _) => new UserAddress
            {
                City = "", Street = "", Number = 0, Zipcode = "",
                Geolocation = new Geolocation { Lat = "0", Long = "0" }
            }))
            .ForMember(dest => dest.Status, opt => opt.MapFrom((src, _, _, _) => src.Status.ToString()))
            .ForMember(dest => dest.Role, opt => opt.MapFrom((src, _, _, _) => src.Role.ToString()));
    }
}
