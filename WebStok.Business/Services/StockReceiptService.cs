using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using WebStok.Business.DTOs;
using WebStok.Business.Exceptions;
using WebStok.Business.Interfaces;
using WebStok.Domain.Entities;
using WebStok.Domain.Interfaces;

namespace WebStok.Business.Services;

public class StockReceiptService : IStockReceiptService
{
    private readonly IRepository<Product> _products;
    private readonly IRepository<Warehouse> _warehouses;
    private readonly IRepository<Lot> _lots;
    private readonly IRepository<StockMovement> _movements;
    private readonly IRepository<StockOperation> _operations;
    private readonly IRepository<UserWarehouse> _warehouseAccess;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<StockReceiptDto> _validator;

    public StockReceiptService(
        IRepository<Product> products,
        IRepository<Warehouse> warehouses,
        IRepository<Lot> lots,
        IRepository<StockMovement> movements,
        IRepository<StockOperation> operations,
        IRepository<UserWarehouse> warehouseAccess,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IValidator<StockReceiptDto> validator)
    {
        _products = products;
        _warehouses = warehouses;
        _lots = lots;
        _movements = movements;
        _operations = operations;
        _warehouseAccess = warehouseAccess;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Guid> ReceiveAsync(
        StockReceiptDto dto,
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
                "Stok girişi yapma yetkiniz yok.");
        }

        var input = new StockReceiptDto
        {
            OperationId = dto.OperationId,
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            LotNumber = dto.LotNumber?.Trim().ToUpperInvariant()
                ?? string.Empty,
            ProductionDate = dto.ProductionDate,
            ExpiryDate = dto.ExpiryDate,
            WarrantyEndDate = dto.WarrantyEndDate,
            Quantity = dto.Quantity,
            Description = dto.Description?.Trim() ?? string.Empty
        };

        await _validator.ValidateAndThrowAsync(
            input, cancellationToken);

        var requestHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(input))));

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (role == UserRole.WarehouseSupervisor)
            {
                var permissions = await _warehouseAccess.ListAsync(
                    x => x.UserId == userId &&
                         x.WarehouseId == input.WarehouseId,
                    ct);

                if (permissions.Count == 0)
                {
                    throw new AccessDeniedException(
                        "Bu depoda işlem yapma yetkiniz yok.");
                }
            }

            var existingOperation = await _operations.GetByIdAsync(
                input.OperationId, ct);

            if (existingOperation is not null)
            {
                if (existingOperation.UserId != userId ||
                    existingOperation.Type != "Receipt" ||
                    existingOperation.RequestHash != requestHash)
                {
                    throw Invalid(
                        nameof(input.OperationId),
                        "İşlem kimliği başka bir işlemde kullanılmış. " +
                        "Yeni bir giriş için sayfayı yeniden açınız.");
                }

                // Aynı işlem zaten kaydedilmiş; yeniden stok ekleme.
                return;
            }

            var warehouse = await _warehouses.GetByIdAsync(
                input.WarehouseId, ct);

            if (warehouse is null || !warehouse.IsActive)
            {
                throw Invalid(
                    nameof(input.WarehouseId),
                    "Depo bulunamadı veya aktif değil.");
            }

            var product = await _products.GetByIdAsync(
                input.ProductId, ct);

            if (product is null || !product.IsActive)
            {
                throw Invalid(
                    nameof(input.ProductId),
                    "Ürün bulunamadı veya aktif değil.");
            }

            if (product.Unit == "Adet" &&
                input.Quantity != decimal.Truncate(input.Quantity))
            {
                throw Invalid(
                    nameof(input.Quantity),
                    "Adet birimli ürünlerde miktar tam sayı olmalıdır.");
            }

            var matchingLots = await _lots.ListAsync(
                x => x.ProductId == input.ProductId &&
                     x.LotNumber == input.LotNumber,
                ct);

            var existingLot = matchingLots.SingleOrDefault();
            Lot lot;

            if (existingLot is null)
            {
                lot = new Lot
                {
                    ProductId = input.ProductId,
                    LotNumber = input.LotNumber,
                    ProductionDate = input.ProductionDate,
                    ExpiryDate = input.ExpiryDate,
                    WarrantyEndDate = input.WarrantyEndDate
                };

                await _lots.AddAsync(lot, ct);
            }
            else
            {
                if (existingLot.ProductionDate != input.ProductionDate ||
                    existingLot.ExpiryDate != input.ExpiryDate ||
                    existingLot.WarrantyEndDate != input.WarrantyEndDate)
                {
                    throw Invalid(
                        nameof(input.LotNumber),
                        "Bu lot zaten mevcut. Tarihler kayıtlı lot " +
                        "bilgileriyle aynı olmalıdır.");
                }

                lot = await _lots.GetByIdAsync(existingLot.Id, ct)
                    ?? throw Invalid(
                        nameof(input.LotNumber),
                        "Lot bulunamadı.");
            }

            var now = DateTime.UtcNow;

            await _operations.AddAsync(new StockOperation
            {
                Id = input.OperationId,
                Type = "Receipt",
                RequestHash = requestHash,
                UserId = userId,
                CreatedAtUtc = now
            }, ct);

            await _movements.AddAsync(new StockMovement
            {
                OperationId = input.OperationId,
                WarehouseId = input.WarehouseId,
                Lot = lot,
                Type = StockMovementType.Receipt,
                Quantity = input.Quantity,
                Description = input.Description,
                PerformedByUserId = userId,
                CreatedAtUtc = now
            }, ct);

        }, cancellationToken);

        return input.OperationId;
    }

    public async Task<StockReceiptPageDataDto> GetPageDataAsync(
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
            "Stok giriş ekranına erişim yetkiniz yok.");
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

    var allowedIds = warehouses.Select(x => x.Id).ToArray();

    var products = await _products.ListAsync(
        cancellationToken: cancellationToken);

    var movements = await _movements.ListAsync(
        x => x.Type == StockMovementType.Receipt &&
             allowedIds.Contains(x.WarehouseId),
        cancellationToken);

    var recent = movements
        .OrderByDescending(x => x.CreatedAtUtc)
        .ThenByDescending(x => x.Id)
        .Take(50)
        .ToList();

    var lotIds = recent.Select(x => x.LotId).Distinct().ToArray();

    var lots = await _lots.ListAsync(
        x => lotIds.Contains(x.Id),
        cancellationToken);

    var productLookup = products.ToDictionary(x => x.Id);
    var warehouseLookup = warehouses.ToDictionary(x => x.Id);
    var lotLookup = lots.ToDictionary(x => x.Id);

    var rows = recent.Select(movement =>
    {
        var lot = lotLookup[movement.LotId];
        var product = productLookup[lot.ProductId];

        return new StockReceiptRowDto
        {
            OperationId = movement.OperationId,
            Product = $"{product.Code} — {product.Name}",
            Warehouse = warehouseLookup[movement.WarehouseId].Name,
            LotNumber = lot.LotNumber,
            Unit = product.Unit,
            Quantity = movement.Quantity,
            CreatedAtUtc = movement.CreatedAtUtc
        };
    }).ToList();

    return new StockReceiptPageDataDto
    {
        Products = products
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new StockChoiceDto
            {
                Id = x.Id,
                Label = $"{x.Code} — {x.Name} ({x.Unit})"
            })
            .ToList(),

        Warehouses = warehouses
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new StockChoiceDto
            {
                Id = x.Id,
                Label = $"{x.Code} — {x.Name}"
            })
            .ToList(),

        RecentReceipts = rows
    };
}

    private static ValidationException Invalid(
        string property,
        string message)
    {
        return new ValidationException(new[]
        {
            new ValidationFailure(property, message)
        });
    }
}