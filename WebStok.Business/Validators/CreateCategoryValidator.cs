using FluentValidation;
using WebStok.Business.DTOs;

namespace WebStok.Business.Validators;

public class CreateCategoryValidator
    : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Kategori adı zorunludur.")
            .MaximumLength(150)
            .WithMessage("Kategori adı en fazla 150 karakter olabilir.");

        RuleFor(x => x.ParentId)
            .GreaterThan(0)
            .When(x => x.ParentId.HasValue)
            .WithMessage("Geçerli bir üst kategori seçiniz.");
    }
}