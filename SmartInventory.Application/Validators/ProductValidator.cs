namespace SmartInventory.Application.Validators;

using FluentValidation;
using SmartInventory.Application.DTOs.Product;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Product code is required").MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required").MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Product price must be greater than 0");
        RuleFor(x => x.QuantityInStock).GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Category is required");
        RuleFor(x => x.SupplierId).GreaterThan(0).WithMessage("Supplier is required");
    }
}

public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Product code is required").MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required").MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Product price must be greater than 0");
        RuleFor(x => x.QuantityInStock).GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Category is required");
        RuleFor(x => x.SupplierId).GreaterThan(0).WithMessage("Supplier is required");
    }
}
