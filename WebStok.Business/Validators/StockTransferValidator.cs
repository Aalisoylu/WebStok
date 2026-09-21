using FluentValidation;
using WebStok.Business.DTOs;

namespace WebStok.Business.Validators;

public class StockTransferValidator
    : AbstractValidator<StockTransferDto>
{
    public StockTransferValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty()
            .WithMessage("İşlem kimliği eksik. Sayfayı yenileyiniz.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Ürün seçiniz.");

        RuleFor(x => x.SourceWarehouseId)
            .GreaterThan(0)
            .WithMessage("Kaynak depo seçiniz.");

        RuleFor(x => x.TargetWarehouseId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .WithMessage("Hedef depo seçiniz.")
            .NotEqual(x => x.SourceWarehouseId)
            .WithMessage("Kaynak ve hedef depo aynı olamaz.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Transfer miktarı sıfırdan büyük olmalıdır.")
            .PrecisionScale(18, 3, true)
            .WithMessage(
                "Miktar en fazla 15 tam ve 3 ondalık basamak içerebilir.");

        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Transfer açıklaması zorunludur.")
            .MaximumLength(500)
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}