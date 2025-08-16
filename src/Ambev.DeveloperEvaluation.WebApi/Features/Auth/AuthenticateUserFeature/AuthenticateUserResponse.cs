namespace Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;

/// <summary>
/// Represents the response returned after user authentication
/// </summary>
public sealed class AuthenticateUserResponse
{
    /// <summary>
    /// Gets or sets the JWT token for authenticated user
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
