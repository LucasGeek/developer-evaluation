using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;

/// <summary>
/// Validator for AuthenticateUserRequest
/// </summary>
public class AuthenticateUserRequestValidator : AbstractValidator<AuthenticateUserRequest>
{
    /// <summary>
    /// Initializes validation rules for AuthenticateUserRequest
    /// </summary>
    public AuthenticateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
}
