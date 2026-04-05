using DeveloperEvaluation.Application.Users.CreateUser;
using DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using AutoMapper;

namespace DeveloperEvaluation.WebApi.Mappings;

public class CreateUserRequestProfile : Profile
{
    public CreateUserRequestProfile()
    {
        CreateMap<CreateUserRequest, CreateUserCommand>();
    }
}