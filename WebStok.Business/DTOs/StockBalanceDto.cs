namespace WebStok.Business.DTOs;

public class StockBalanceDto
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }

    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public decimal WarehouseProductQuantity { get; set; }
    public decimal MinimumStockLevel { get; set; }

    public DateOnly? ExpiryDate { get; set; }
    public DateOnly? WarrantyEndDate { get; set; }

    public bool IsCritical =>
        WarehouseProductQuantity <= MinimumStockLevel;
}