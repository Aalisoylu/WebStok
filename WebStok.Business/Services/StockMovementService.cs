using WebStok.Business.DTOs;
using WebStok.Business.Exceptions;
using WebStok.Business.Interfaces;
using WebStok.Domain.Entities;
using WebStok.Domain.Interfaces;

namespace WebStok.Business.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IRepository<StockMovement> _movements;
    private readonly IRepository<Warehouse> _warehouses;
    private readonly IRepository<Lot> _lots;
    private readonly IRepository<Product> _products;
    private readonly IRepository<AppUser> _users;
    private readonly IRepository<UserWarehouse> _warehouseAccess;
    private readonly ICurrentUser _currentUser;

    public StockMovementService(
        IRepository<StockMovement> movements,
        IRepository<Warehouse> warehouses,
        IRepository<Lot> lots,
        IRepository<Product> products,
        IRepository<AppUser> users,
        IRepository<UserWarehouse> warehouseAccess,
        ICurrentUser currentUser)
    {
        _movements = movements;
        _warehouses = warehouses;
        _lots = lots;
        _products = products;
        _users = users;
        _warehouseAccess = warehouseAccess;
        _currentUser = currentUser;
    }

    public async Task<List<StockMovementListDto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId
            ?? throw new AccessDeniedException("Oturum açmalısınız.");

        var role = _currentUser.Role;

        if (role is not (
            UserRole.SuperAdmin or
            UserRole.ITAdmin or
            UserRole.WarehouseSupervisor))
        {
            throw new AccessDeniedException(
                "Stok hareketlerine erişim yetkiniz yok.");
        }

        var warehouses = await _warehouses.ListAsync(
            cancellationToken: cancellationToken);

        if (role == UserRole.WarehouseSupervisor)
        {
            var permissions = await _warehouseAccess.ListAsync(
                x => x.UserId == userId,
                cancellationToken);

            var permittedIds = permissions
                .Select(x => x.WarehouseId)
                .ToHashSet();

            warehouses = warehouses
                .Where(x => permittedIds.Contains(x.Id))
                .ToList();
        }

        var warehouseIds = warehouses.Select(x => x.Id).ToArray();

        var movements = await _movements.ListAsync(
            x => warehouseIds.Contains(x.WarehouseId),
            cancellationToken);

        var returnedByMovement = movements
            .Where(x => x.Type == StockMovementType.ReturnIn && x.RelatedMovementId.HasValue)
            .GroupBy(x => x.RelatedMovementId!.Value)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity));

        var recent = movements
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(200)
            .ToList();

        var lotIds = recent.Select(x => x.LotId).Distinct().ToArray();

        var lots = await _lots.ListAsync(
            x => lotIds.Contains(x.Id),
            cancellationToken);

        var productIds = lots
            .Select(x => x.ProductId)
            .Distinct()
            .ToArray();

        var products = await _products.ListAsync(
            x => productIds.Contains(x.Id),
            cancellationToken);

        var userIds = recent
            .Select(x => x.PerformedByUserId)
            .Distinct()
            .ToArray();

        var users = await _users.ListAsync(
            x => userIds.Contains(x.Id),
            cancellationToken);

        var warehouseLookup = warehouses.ToDictionary(x => x.Id);
        var lotLookup = lots.ToDictionary(x => x.Id);
        var productLookup = products.ToDictionary(x => x.Id);
        var userLookup = users.ToDictionary(x => x.Id);

        return recent.Select(movement =>
        {
            var lot = lotLookup[movement.LotId];
            var product = productLookup[lot.ProductId];

            return new StockMovementListDto
            {
                Id = movement.Id,
                CanReturn = movement.Quantity < 0 && returnedByMovement.GetValueOrDefault(movement.Id) < -movement.Quantity && (movement.Type == StockMovementType.ProductionIssue || movement.Type == StockMovementType.ServiceIssue),
                OperationId = movement.OperationId,
                CreatedAtUtc = movement.CreatedAtUtc,
                ProductCode = product.Code,
                ProductName = product.Name,
                WarehouseName =
                    warehouseLookup[movement.WarehouseId].Name,
                LotNumber = lot.LotNumber,
                MovementType = GetTypeName(movement.Type),
                PerformedBy =
                    userLookup[movement.PerformedByUserId].DisplayName,
                Description = movement.Description,
                Unit = product.Unit,
                Quantity = movement.Quantity
            };
        }).ToList();
    }

    private static string GetTypeName(StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Receipt => "Mal kabul",
            StockMovementType.ProductionIssue => "Üretim çıkışı",
            StockMovementType.ServiceIssue => "Servis çıkışı",
            StockMovementType.ReturnIn => "İade girişi",
            StockMovementType.DamageOut => "Hasarlı parça düşüşü",
            StockMovementType.TransferIn => "Transfer girişi",
            StockMovementType.TransferOut => "Transfer çıkışı",
            StockMovementType.SupplierReturn => "Tedarikçiye iade",
            _ => "Bilinmeyen hareket"
        };
    }
}
