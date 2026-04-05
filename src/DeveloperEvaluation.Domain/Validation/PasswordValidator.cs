using FluentValidation;

namespace DeveloperEvaluation.Domain.Validation;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        RuleFor(password => password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]+").WithMessage("Password must be at least 8 characters, with letters, numbers, and special characters.")
            .Matches(@"[a-z]+").WithMessage("Password must be at least 8 characters, with letters, numbers, and special characters.")
            .Matches(@"[0-9]+").WithMessage("Password must be at least 8 characters, with letters, numbers, and special characters.")
            .Matches(@"[@$!%*?&]+").WithMessage("Password must be at least 8 characters, with letters, numbers, and special characters.");
    }
}