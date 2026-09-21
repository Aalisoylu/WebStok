namespace WebStok.Domain.Entities;

public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
}