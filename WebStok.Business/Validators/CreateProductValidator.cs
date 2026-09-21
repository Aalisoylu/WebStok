using FluentValidation;
using WebStok.Business.DTOs;

namespace WebStok.Business.Validators;

public class CreateProductValidator
    : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ürün kodu zorunludur.")
            .MaximumLength(60)
            .WithMessage("Ürün kodu en fazla 60 karakter olabilir.")
            .Matches("^[A-Z0-9-]+$")
            .WithMessage(
                "Ürün kodunda yalnızca A-Z, rakam ve tire kullanılabilir.");

        RuleFor(x => x.Barcode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Barkod zorunludur.")
            .MaximumLength(64)
            .WithMessage("Barkod en fazla 64 karakter olabilir.")
            .Matches("^[A-Z0-9-]+$")
            .WithMessage(
                "Barkodda yalnızca A-Z, rakam ve tire kullanılabilir.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ürün adı zorunludur.")
            .MaximumLength(200)
            .WithMessage("Ürün adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Kategori seçiniz.");

        RuleFor(x => x.Unit)
            .Must(x => x is "Adet" or "Kg" or "Metre" or "Litre")
            .WithMessage("Geçerli bir ölçü birimi seçiniz.");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Alış fiyatı negatif olamaz.")
            .PrecisionScale(18, 2, true)
            .WithMessage(
                "Alış fiyatı en fazla 16 tam ve 2 ondalık basamak içerebilir.");

        RuleFor(x => x.SalePrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Satış fiyatı negatif olamaz.")
            .PrecisionScale(18, 2, true)
            .WithMessage(
                "Satış fiyatı en fazla 16 tam ve 2 ondalık basamak içerebilir.");

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum stok negatif olamaz.")
            .PrecisionScale(18, 3, true)
            .WithMessage(
                "Minimum stok en fazla 15 tam ve 3 ondalık basamak içerebilir.");

        RuleFor(x => x.IssueMethod)
            .IsInEnum()
            .WithMessage("Geçerli bir çıkış yöntemi seçiniz.");
    }
}