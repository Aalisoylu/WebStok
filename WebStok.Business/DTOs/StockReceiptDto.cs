namespace WebStok.Business.DTOs;

public class StockReceiptDto
{
    public Guid OperationId { get; set; } = Guid.NewGuid();

    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public string LotNumber { get; set; } = string.Empty;

    public DateOnly? ProductionDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateOnly? WarrantyEndDate { get; set; }

    public decimal Quantity { get; set; }

    public string Description { get; set; } = string.Empty;
}