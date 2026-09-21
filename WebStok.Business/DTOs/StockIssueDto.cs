namespace WebStok.Business.DTOs;

public enum StockIssuePurpose
{
    Production = 1,
    Service = 2
}

public class StockIssueDto
{
    public Guid OperationId { get; set; } = Guid.NewGuid();

    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public StockIssuePurpose Purpose { get; set; }
        = StockIssuePurpose.Production;

    public decimal Quantity { get; set; }

    public string Description { get; set; } = string.Empty;
}