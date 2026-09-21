namespace WebStok.Business.DTOs;

public class StockMovementListDto
{
    public long Id { get; set; }
    public bool CanReturn { get; set; }
    public Guid OperationId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
}
