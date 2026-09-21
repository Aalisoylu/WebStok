using WebStok.Domain.Entities;

namespace WebStok.Business.DTOs;

public class CreateProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string Unit { get; set; } = "Adet";

    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal MinimumStockLevel { get; set; }

    public StockIssueMethod IssueMethod { get; set; }
        = StockIssueMethod.Fefo;
}

public class ProductListDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryPath { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal MinimumStockLevel { get; set; }

    public StockIssueMethod IssueMethod { get; set; }
    public bool IsActive { get; set; }
}