namespace WebStok.Domain.Entities;

public class StockOperation
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    public int UserId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}