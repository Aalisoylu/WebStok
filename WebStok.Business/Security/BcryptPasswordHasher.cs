using WebStok.Business.Interfaces;

namespace WebStok.Business.Security;

public class BcryptPasswordHasher : IPasswordHasher
{
    private static readonly string DummyHash =
        BCrypt.Net.BCrypt.EnhancedHashPassword(
            Guid.NewGuid().ToString("N"),
            workFactor: 12);

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            password.Length < 12 ||
            password.Length > 128)
        {
            throw new ArgumentException(
                "Şifre 12–128 karakter arasında olmalıdır.",
                nameof(password));
        }

        return BCrypt.Net.BCrypt.EnhancedHashPassword(
            password,
            workFactor: 12);
    }

    public bool Verify(string password, string? passwordHash)
    {
        if (string.IsNullOrEmpty(password) || password.Length > 128)
        {
            return false;
        }

        var hash = string.IsNullOrWhiteSpace(passwordHash)
            ? DummyHash
            : passwordHash;

        return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
    }
}