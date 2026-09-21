using FluentValidation;
using WebStok.Business.DTOs;

namespace WebStok.Business.Validators;

public class StockReceiptValidator
    : AbstractValidator<StockReceiptDto>
{
    public StockReceiptValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty()
            .WithMessage("İşlem kimliği eksik. Sayfayı yenileyiniz.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Ürün seçiniz.");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0)
            .WithMessage("Depo seçiniz.");

        RuleFor(x => x.LotNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Lot numarası zorunludur.")
            .MaximumLength(60)
            .WithMessage("Lot numarası en fazla 60 karakter olabilir.")
            .Matches("^[A-Z0-9-]+$")
            .WithMessage(
                "Lot numarasında yalnızca A-Z, rakam ve tire kullanılabilir.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Giriş miktarı sıfırdan büyük olmalıdır.")
            .PrecisionScale(18, 3, true)
            .WithMessage(
                "Miktar en fazla 15 tam ve 3 ondalık basamak içerebilir.");

        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("İşlem açıklaması zorunludur.")
            .MaximumLength(500)
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.ExpiryDate)
            .Must((dto, expiry) =>
                !dto.ProductionDate.HasValue ||
                !expiry.HasValue ||
                expiry.Value >= dto.ProductionDate.Value)
            .WithMessage(
                "Son kullanma tarihi üretim tarihinden önce olamaz.");

        RuleFor(x => x.WarrantyEndDate)
            .Must((dto, warrantyEnd) =>
                !dto.ProductionDate.HasValue ||
                !warrantyEnd.HasValue ||
                warrantyEnd.Value >= dto.ProductionDate.Value)
            .WithMessage(
                "Garanti bitiş tarihi üretim tarihinden önce olamaz.");
    }
}