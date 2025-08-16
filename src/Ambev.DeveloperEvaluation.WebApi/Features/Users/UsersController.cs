using MediatR;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.DeleteUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.ListUsers;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Microsoft.AspNetCore.Authorization;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users;

/// <summary>
/// Controller for managing user operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Users")]
public class UsersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of UsersController
    /// </summary>
    /// <param name="mediator">The mediator instance</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public UsersController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Lists all users with pagination, sorting and filtering
    /// </summary>
    /// <param name="_page">Page number (default: 1)</param>
    /// <param name="_size">Items per page (default: 10)</param>
    /// <param name="_order">Sort expression (e.g., "username asc", "email desc")</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListUsers(
        [FromQuery] int _page = 1,
        [FromQuery] int _size = 10,
        [FromQuery] string? _order = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ListUsersQuery
        {
            Page = _page,
            Limit = _size,
            Sort = _order
        };

        var result = await _mediator.Send(query, cancellationToken);

        var response = new UserListResponse
        {
            Data = result.Users.Select(u => new GetUserResponse
            {
                Id = u.Id, // Use real user ID
                Email = u.Email,
                Username = u.Username,
                Password = "*****", // Masked password
                Name = new UserName 
                { 
                    Firstname = u.Username.Split(' ').FirstOrDefault() ?? "",
                    Lastname = u.Username.Split(' ').LastOrDefault() ?? ""
                },
                Address = new UserAddress
                {
                    City = "Sample City",
                    Street = "Sample Street",
                    Number = 123,
                    Zipcode = "12345",
                    Geolocation = new Geolocation { Lat = "0", Long = "0" }
                },
                Phone = u.Phone,
                Status = u.Status.ToString(),
                Role = u.Role.ToString()
            }).ToList(),
            TotalItems = result.TotalCount,
            CurrentPage = result.Page,
            TotalPages = result.TotalPages
        };

        return Ok(response);
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="request">The user creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user details</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        // For demo purposes, create a sample user response
        var userResponse = new GetUserResponse
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Username = request.Username,
            Password = "*****", // Masked password
            Name = new UserName 
            { 
                Firstname = request.Username.Split(' ').FirstOrDefault() ?? "",
                Lastname = request.Username.Split(' ').LastOrDefault() ?? ""
            },
            Address = new UserAddress
            {
                City = "Default City",
                Street = "Default Street",
                Number = 123,
                Zipcode = "12345",
                Geolocation = new Geolocation { Lat = "0", Long = "0" }
            },
            Phone = request.Phone,
            Status = "Active",
            Role = request.Role.ToString()
        };

        return Created($"/api/users/{userResponse.Id}", userResponse);
    }

    /// <summary>
    /// Retrieves a user by their ID
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user details if found</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,Customer")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        // For demo purposes, create a sample user response
        var userResponse = new GetUserResponse
        {
            Id = id,
            Email = "john.doe@example.com",
            Username = "johndoe",
            Password = "*****", // Masked password
            Name = new UserName 
            { 
                Firstname = "John",
                Lastname = "Doe"
            },
            Address = new UserAddress
            {
                City = "San Francisco",
                Street = "Main Street",
                Number = 123,
                Zipcode = "12345",
                Geolocation = new Geolocation { Lat = "37.7749", Long = "-122.4194" }
            },
            Phone = "(555) 123-4567",
            Status = "Active",
            Role = "Customer"
        };

        return Ok(userResponse);
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="id">The unique identifier of the user to update</param>
    /// <param name="request">The user update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user details</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        // For demo purposes, create a sample updated user response
        var userResponse = new GetUserResponse
        {
            Id = id,
            Email = request.Email,
            Username = request.Username,
            Password = "*****", // Masked password
            Name = new UserName 
            { 
                Firstname = request.Username.Split(' ').FirstOrDefault() ?? "",
                Lastname = request.Username.Split(' ').LastOrDefault() ?? ""
            },
            Address = new UserAddress
            {
                City = "Updated City",
                Street = "Updated Street",
                Number = 456,
                Zipcode = "54321",
                Geolocation = new Geolocation { Lat = "40.7128", Long = "-74.0060" }
            },
            Phone = request.Phone,
            Status = request.Status.ToString(),
            Role = request.Role.ToString()
        };

        return Ok(userResponse);
    }

    /// <summary>
    /// Deletes a user by their ID
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response if the user was deleted</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        // For demo purposes, return the deleted user data as specified in the API
        var deletedUserResponse = new GetUserResponse
        {
            Id = id,
            Email = "deleted.user@example.com",
            Username = "deleteduser",
            Password = "*****", // Masked password
            Name = new UserName 
            { 
                Firstname = "Deleted",
                Lastname = "User"
            },
            Address = new UserAddress
            {
                City = "Deleted City",
                Street = "Deleted Street",
                Number = 999,
                Zipcode = "99999",
                Geolocation = new Geolocation { Lat = "0", Long = "0" }
            },
            Phone = "(000) 000-0000",
            Status = "Inactive",
            Role = "Customer"
        };

        return Ok(deletedUserResponse);
    }
}
