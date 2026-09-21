using FluentValidation;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Domain.Entities;
using WebStok.Domain.Interfaces;

namespace WebStok.Business.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<AppUser> _users;
    private readonly IRepository<UserSession> _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<LoginDto> _validator;

    public AuthService(
        IRepository<AppUser> users,
        IRepository<UserSession> sessions,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IValidator<LoginDto> validator)
    {
        _users = users;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<AuthSessionDto?> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var input = new LoginDto
        {
            Username = dto.Username?.Trim() ?? string.Empty,
            Password = dto.Password ?? string.Empty
        };

        await _validator.ValidateAndThrowAsync(
            input, cancellationToken);

        var normalizedUsername = input.Username.ToUpperInvariant();

        var matches = await _users.ListAsync(
            x => x.NormalizedUsername == normalizedUsername,
            cancellationToken);

        var user = matches.SingleOrDefault();

        var passwordMatches = _passwordHasher.Verify(
            input.Password,
            user?.PasswordHash);

        if (user is null ||
            !passwordMatches ||
            !user.IsActive ||
            !Enum.IsDefined(typeof(UserRole), user.Role))
        {
            return null;
        }

        var now = DateTime.UtcNow;

        var session = new UserSession
        {
            UserId = user.Id,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddHours(8)
        };

        await _sessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(user, session);
    }

    public async Task<AuthSessionDto?> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetByIdAsync(
            sessionId, cancellationToken);

        if (session is null ||
            session.RevokedAtUtc.HasValue ||
            session.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return null;
        }

        var user = await _users.GetByIdAsync(
            session.UserId, cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !Enum.IsDefined(typeof(UserRole), user.Role))
        {
            return null;
        }

        return ToDto(user, session);
    }

    public async Task LogoutAsync(
        Guid sessionId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetByIdAsync(
            sessionId, cancellationToken);

        if (session is null ||
            session.UserId != userId ||
            session.RevokedAtUtc.HasValue)
        {
            return;
        }

        session.RevokedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AuthSessionDto ToDto(
        AppUser user,
        UserSession session)
    {
        return new AuthSessionDto
        {
            SessionId = session.Id,
            UserId = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Role = user.Role,
            ExpiresAtUtc = session.ExpiresAtUtc
        };
    }
}