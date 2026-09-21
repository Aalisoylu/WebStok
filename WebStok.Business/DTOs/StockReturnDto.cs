namespace WebStok.Business.DTOs;

public class StockReturnDto
{
    public Guid OperationId { get; set; } = Guid.NewGuid();

    public long OriginalMovementId { get; set; }

    public decimal Quantity { get; set; }

    public string Description { get; set; } = string.Empty;
}