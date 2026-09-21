namespace WebStok.Domain.Entities;

public enum StockMovementType
{
    Receipt = 1,
    ProductionIssue = 2,
    ServiceIssue = 3,
    ReturnIn = 4,
    DamageOut = 5,
    TransferIn = 6,
    TransferOut = 7,
    SupplierReturn = 8
}

public class StockMovement
{
    public long Id { get; set; }

    public long? RelatedMovementId { get; set; }

    public Guid OperationId { get; set; }
    //Depo giriş ve çıkışlar için kullanılacak

    public int WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public int LotId { get; set; }

    public Lot Lot { get; set; } = null!;

    public StockMovementType Type { get; set; }

    public decimal Quantity { get; set; }

    public string Description { get; set; } = string.Empty;

    public int PerformedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;
}