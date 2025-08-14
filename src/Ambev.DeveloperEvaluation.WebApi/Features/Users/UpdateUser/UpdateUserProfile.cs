using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUser;

/// <summary>
/// Profile for mapping UpdateUser request DTOs
/// </summary>
public class UpdateUserProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for UpdateUser operation
    /// </summary>
    public UpdateUserProfile()
    {
        CreateMap<UpdateUserRequest, UpdateUserCommand>();
    }
}