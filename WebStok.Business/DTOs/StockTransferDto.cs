namespace WebStok.Business.DTOs;

public class StockTransferDto
{
    public Guid OperationId { get; set; } = Guid.NewGuid();

    public int ProductId { get; set; }

    public int SourceWarehouseId { get; set; }

    public int TargetWarehouseId { get; set; }

    public decimal Quantity { get; set; }

    public string Description { get; set; } = string.Empty;
}