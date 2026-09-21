using FluentValidation;
using WebStok.Business.DTOs;

namespace WebStok.Business.Validators;

public class CreateWarehouseValidator
    : AbstractValidator<CreateWarehouseDto>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Depo kodu zorunludur.")
            .MaximumLength(30)
            .WithMessage("Depo kodu en fazla 30 karakter olabilir.")
            .Matches("^[A-Z0-9-]+$")
            .WithMessage(
                "Depo kodunda yalnızca A-Z, rakam ve tire kullanılabilir.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Depo adı zorunludur.")
            .MaximumLength(150)
            .WithMessage("Depo adı en fazla 150 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}