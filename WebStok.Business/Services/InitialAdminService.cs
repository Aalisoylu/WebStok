using WebStok.Business.Interfaces;
using WebStok.Domain.Entities;
using WebStok.Domain.Interfaces;

namespace WebStok.Business.Services;

public class InitialAdminService : IInitialAdminService
{
    private readonly IRepository<AppUser> _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public InitialAdminService(
        IRepository<AppUser> users,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> EnsureCreatedAsync(
        string? password,
        CancellationToken cancellationToken = default)
    {
        var existingUsers = await _users.ListAsync(
            cancellationToken: cancellationToken);

        if (existingUsers.Count > 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "İlk kurulum için Bootstrap:AdminPassword ayarlanmalıdır.");
        }

        var admin = new AppUser
        {
            Username = "admin",
            NormalizedUsername = "ADMIN",
            DisplayName = "Sistem Yöneticisi",
            PasswordHash = _passwordHasher.Hash(password),
            Role = UserRole.SuperAdmin,
            IsActive = true
        };

        await _users.AddAsync(admin, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}