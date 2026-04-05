using MediatR;

namespace DeveloperEvaluation.Application.Auth.AuthenticateUser;

/// <summary>
/// Command for authenticating a user in the system.
/// Implements IRequest for mediator pattern handling.
/// </summary>
public class AuthenticateUserCommand : IRequest<AuthenticateUserResult>
{
    /// <summary>
    /// Gets or sets the username for authentication.
    /// Can be email or username. Used as the primary identifier for the user.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for authentication.
    /// Will be verified against the stored hashed password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
