using WebStok.Business.DTOs;

namespace WebStok.Business.Interfaces;

public interface IAuthService
{
    Task<AuthSessionDto?> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default);

    Task<AuthSessionDto?> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        Guid sessionId,
        int userId,
        CancellationToken cancellationToken = default);
}