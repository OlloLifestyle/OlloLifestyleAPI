using FluentValidation;
using OlloLifestyleAPI.Application.DTOs.Master;

namespace OlloLifestyleAPI.Application.Validators.Master;

public class CreateCompanyRequestValidator : AbstractValidator<CreateCompanyRequest>
{
    public CreateCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters");

        RuleFor(x => x.DatabaseName)
            .NotEmpty().WithMessage("Database name is required")
            .MaximumLength(50).WithMessage("Database name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Database name can only contain letters, numbers, and underscores");

        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("Connection string is required")
            .MaximumLength(500).WithMessage("Connection string cannot exceed 500 characters");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));
    }
}

public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));
    }
}