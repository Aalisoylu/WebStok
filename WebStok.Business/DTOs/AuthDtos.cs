using WebStok.Domain.Entities;

namespace WebStok.Business.DTOs;

public class LoginDto
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class AuthSessionDto
{
    public Guid SessionId { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
}