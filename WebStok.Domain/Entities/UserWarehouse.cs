namespace WebStok.Domain.Entities;

public class UserWarehouse
{
    public int UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public int WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
}