using WebStok.Business.DTOs;
using WebStok.Business.Exceptions;
using WebStok.Business.Interfaces;
using WebStok.Domain.Entities;
using WebStok.Domain.Interfaces;

namespace WebStok.Business.Services;

public class StockBalanceService : IStockBalanceService
{
    private readonly IRepository<Product> _products;
    private readonly IRepository<Warehouse> _warehouses;
    private readonly IRepository<Lot> _lots;
    private readonly IRepository<StockMovement> _movements;
    private readonly IRepository<UserWarehouse> _warehouseAccess;
    private readonly ICurrentUser _currentUser;

    public StockBalanceService(
        IRepository<Product> products,
        IRepository<Warehouse> warehouses,
        IRepository<Lot> lots,
        IRepository<StockMovement> movements,
        IRepository<UserWarehouse> warehouseAccess,
        ICurrentUser currentUser)
    {
        _products = products;
        _warehouses = warehouses;
        _lots = lots;
        _movements = movements;
        _warehouseAccess = warehouseAccess;
        _currentUser = currentUser;
    }

    public async Task<List<StockBalanceDto>> ListAsync(
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
                "Stok bilgilerine erişim yetkiniz yok.");
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

        var lotIds = movements
            .Select(x => x.LotId)
            .Distinct()
            .ToArray();

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

        var warehouseLookup = warehouses.ToDictionary(x => x.Id);
        var lotLookup = lots.ToDictionary(x => x.Id);
        var productLookup = products.ToDictionary(x => x.Id);

        var balances = movements
            .GroupBy(x => new { x.WarehouseId, x.LotId })
            .Select(group => new
            {
                group.Key.WarehouseId,
                group.Key.LotId,
                Quantity = group.Sum(x => x.Quantity)
            })
            .ToList();

        var warehouseTotals = balances
            .GroupBy(x => new
            {
                x.WarehouseId,
                ProductId = lotLookup[x.LotId].ProductId
            })
            .ToDictionary(
                group => (group.Key.WarehouseId, group.Key.ProductId),
                group => group.Sum(x => x.Quantity));

        return balances.Select(balance =>
        {
            var lot = lotLookup[balance.LotId];
            var product = productLookup[lot.ProductId];

            return new StockBalanceDto
            {
                ProductId = product.Id,
                WarehouseId = balance.WarehouseId,
                ProductCode = product.Code,
                ProductName = product.Name,
                WarehouseName =
                    warehouseLookup[balance.WarehouseId].Name,
                LotNumber = lot.LotNumber,
                Unit = product.Unit,
                Quantity = balance.Quantity,
                WarehouseProductQuantity =
                    warehouseTotals[(balance.WarehouseId, product.Id)],
                MinimumStockLevel = product.MinimumStockLevel,
                ExpiryDate = lot.ExpiryDate,
                WarrantyEndDate = lot.WarrantyEndDate
            };
        })
        .OrderBy(x => x.WarehouseName)
        .ThenBy(x => x.ProductCode)
        .ThenBy(x => x.ExpiryDate ?? DateOnly.MaxValue)
        .ThenBy(x => x.LotNumber)
        .ToList();
    }
}