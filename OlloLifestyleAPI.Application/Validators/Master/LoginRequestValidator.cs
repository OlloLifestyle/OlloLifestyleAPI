using FluentValidation;
using OlloLifestyleAPI.Application.DTOs.Master;

namespace OlloLifestyleAPI.Application.Validators.Master;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(100).WithMessage("Username cannot exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long");
    }
}