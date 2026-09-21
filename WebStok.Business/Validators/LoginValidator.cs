using FluentValidation;
using WebStok.Business.DTOs;

namespace WebStok.Business.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Kullanıcı adı zorunludur.")
            .MaximumLength(50)
            .WithMessage("Kullanıcı adı en fazla 50 karakter olabilir.")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Kullanıcı adı geçersiz karakter içeriyor.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MaximumLength(128)
            .WithMessage("Şifre en fazla 128 karakter olabilir.");
    }
}