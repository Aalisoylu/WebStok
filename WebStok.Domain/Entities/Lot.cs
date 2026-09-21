namespace WebStok.Domain.Entities;

public class Lot
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public string LotNumber { get; set; } = string.Empty;

    public DateOnly? ProductionDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateOnly? WarrantyEndDate { get; set; }

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;
}