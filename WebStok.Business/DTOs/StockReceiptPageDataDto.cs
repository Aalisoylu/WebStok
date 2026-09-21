namespace WebStok.Business.DTOs;

public class StockChoiceDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class StockReceiptRowDto
{
    public Guid OperationId { get; set; }
    public string Product { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class StockReceiptPageDataDto
{
    public List<StockChoiceDto> Products { get; set; } = new();
    public List<StockChoiceDto> Warehouses { get; set; } = new();
    public List<StockReceiptRowDto> RecentReceipts { get; set; } = new();
}