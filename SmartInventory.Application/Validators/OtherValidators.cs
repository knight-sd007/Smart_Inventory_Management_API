namespace SmartInventory.Application.Validators;

using FluentValidation;
using SmartInventory.Application.DTOs.Category;
using SmartInventory.Application.DTOs.Supplier;
using SmartInventory.Application.DTOs.Auth;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Category name is required").MaximumLength(100);
    }
}

public class CreateSupplierValidator : AbstractValidator<CreateSupplierDto>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Supplier name is required").MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required");
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required").MaximumLength(20);
        RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required");
    }
}

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
    }
}

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required").MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required").MinimumLength(6).WithMessage("Password must be at least 6 characters");
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full name is required");
    }
}
