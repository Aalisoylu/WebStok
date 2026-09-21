using FluentValidation;
using WebStok.Business.DTOs;

namespace WebStok.Business.Validators;

public class StockIssueValidator
    : AbstractValidator<StockIssueDto>
{
    public StockIssueValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty()
            .WithMessage("İşlem kimliği eksik. Sayfayı yenileyiniz.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Ürün seçiniz.");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0)
            .WithMessage("Kaynak depo seçiniz.");

        RuleFor(x => x.Purpose)
            .IsInEnum()
            .WithMessage("Geçerli bir çıkış amacı seçiniz.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Çıkış miktarı sıfırdan büyük olmalıdır.")
            .PrecisionScale(18, 3, true)
            .WithMessage(
                "Miktar en fazla 15 tam ve 3 ondalık basamak içerebilir.");

        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Çıkış açıklaması zorunludur.")
            .MaximumLength(500)
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}