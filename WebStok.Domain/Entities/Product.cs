namespace WebStok.Domain.Entities;

public enum StockIssueMethod
{
    Fifo = 1,
    Fefo = 2
}

public class Product
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public string Unit { get; set; } = "Adet";

    public decimal PurchasePrice { get; set; }

    public decimal SalePrice { get; set; }

    public decimal MinimumStockLevel { get; set; }

    public StockIssueMethod IssueMethod { get; set; }
        = StockIssueMethod.Fefo;

    public bool IsActive { get; set; } = true;

    public ICollection<Lot> Lots { get; set; }
        = new List<Lot>();
}

//ürün/parça kartı