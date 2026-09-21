namespace WebStok.Business.Interfaces;

public interface IInitialAdminService
{
    Task<bool> EnsureCreatedAsync(
        string? password,
        CancellationToken cancellationToken = default);
}