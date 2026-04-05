using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DeveloperEvaluation.WebApi.Common;
using DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;
using DeveloperEvaluation.Application.Auth.AuthenticateUser;
using DeveloperEvaluation.Common.Validation;

namespace DeveloperEvaluation.WebApi.Features.Auth;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Authentication")]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of AuthController
    /// </summary>
    /// <param name="mediator">The mediator instance</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public AuthController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Authenticates a user with their credentials
    /// </summary>
    /// <param name="request">The login request containing username and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT authentication token if credentials are valid</returns>
    /// <response code="200">Successfully authenticated - returns JWT token</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseWithData<AuthenticateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] AuthenticateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var validator = new AuthenticateUserRequestValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var command = _mapper.Map<AuthenticateUserCommand>(request);
            var response = await _mediator.Send(command, cancellationToken);

            return Ok(new ApiResponseWithData<AuthenticateUserResponse>
            {
                Success = true,
                Message = "User authenticated successfully",
                Data = _mapper.Map<AuthenticateUserResponse>(response)
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { new ValidationErrorDetail { Error = "Authentication", Detail = "Authentication failed" } }
            });
        }
    }
}
