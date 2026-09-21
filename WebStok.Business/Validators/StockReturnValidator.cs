using FluentValidation;
using WebStok.Business.DTOs;

namespace WebStok.Business.Validators;

public class StockReturnValidator
    : AbstractValidator<StockReturnDto>
{
    public StockReturnValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty()
            .WithMessage("İşlem kimliği eksik. Sayfayı yenileyiniz.");

        RuleFor(x => x.OriginalMovementId)
            .GreaterThan(0)
            .WithMessage("İadenin bağlı olduğu çıkış hareketini seçiniz.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("İade miktarı sıfırdan büyük olmalıdır.")
            .PrecisionScale(18, 3, true)
            .WithMessage(
                "Miktar en fazla 15 tam ve 3 ondalık basamak içerebilir.");

        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("İade açıklaması zorunludur.")
            .MaximumLength(500)
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}