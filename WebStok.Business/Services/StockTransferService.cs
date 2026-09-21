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

public class StockTransferService : IStockTransferService
{
    private readonly IRepository<Product> _products;
    private readonly IRepository<Warehouse> _warehouses;
    private readonly IRepository<Lot> _lots;
    private readonly IRepository<StockMovement> _movements;
    private readonly IRepository<StockOperation> _operations;
    private readonly IRepository<UserWarehouse> _warehouseAccess;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<StockTransferDto> _validator;

    public StockTransferService(
        IRepository<Product> products,
        IRepository<Warehouse> warehouses,
        IRepository<Lot> lots,
        IRepository<StockMovement> movements,
        IRepository<StockOperation> operations,
        IRepository<UserWarehouse> warehouseAccess,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IValidator<StockTransferDto> validator)
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

    public async Task<Guid> TransferAsync(
        StockTransferDto dto,
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
                "Transfer yapma yetkiniz yok.");
        }

        var input = new StockTransferDto
        {
            OperationId = dto.OperationId,
            ProductId = dto.ProductId,
            SourceWarehouseId = dto.SourceWarehouseId,
            TargetWarehouseId = dto.TargetWarehouseId,
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
                         (x.WarehouseId == input.SourceWarehouseId ||
                          x.WarehouseId == input.TargetWarehouseId),
                    ct);

                var allowedIds = permissions
                    .Select(x => x.WarehouseId)
                    .ToHashSet();

                if (!allowedIds.Contains(input.SourceWarehouseId) ||
                    !allowedIds.Contains(input.TargetWarehouseId))
                {
                    throw new AccessDeniedException(
                        "Transfer için kaynak ve hedef depoya " +
                        "erişim yetkiniz olmalıdır.");
                }
            }

            var existingOperation = await _operations.GetByIdAsync(
                input.OperationId, ct);

            if (existingOperation is not null)
            {
                if (existingOperation.UserId != userId ||
                    existingOperation.Type != "Transfer" ||
                    existingOperation.RequestHash != requestHash)
                {
                    throw Invalid(
                        nameof(input.OperationId),
                        "İşlem kimliği başka bir işlemde kullanılmış. " +
                        "Yeni bir transfer için sayfayı yeniden açınız.");
                }

                // Aynı transfer zaten kaydedilmiş.
                return;
            }

            var source = await _warehouses.GetByIdAsync(
                input.SourceWarehouseId, ct);

            if (source is null || !source.IsActive)
            {
                throw Invalid(
                    nameof(input.SourceWarehouseId),
                    "Kaynak depo bulunamadı veya aktif değil.");
            }

            var target = await _warehouses.GetByIdAsync(
                input.TargetWarehouseId, ct);

            if (target is null || !target.IsActive)
            {
                throw Invalid(
                    nameof(input.TargetWarehouseId),
                    "Hedef depo bulunamadı veya aktif değil.");
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

            var lots = await _lots.ListAsync(
                x => x.ProductId == input.ProductId,
                ct);

            var lotIds = lots.Select(x => x.Id).ToArray();

            var sourceMovements = await _movements.ListAsync(
                x => x.WarehouseId == input.SourceWarehouseId &&
                     lotIds.Contains(x.LotId),
                ct);

            var balances = sourceMovements
                .GroupBy(x => x.LotId)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        Quantity = group.Sum(x => x.Quantity),

                        FirstReceivedAtUtc = group
                            .Where(x => x.Quantity > 0)
                            .Select(x => x.CreatedAtUtc)
                            .DefaultIfEmpty(DateTime.MaxValue)
                            .Min()
                    });

            var now = DateTime.UtcNow;

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows()
                    ? "Turkey Standard Time"
                    : "Europe/Istanbul");

            var today = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTimeFromUtc(now, timeZone));

            var candidates = new List<AvailableLot>();

            foreach (var lot in lots)
            {
                if (lot.ExpiryDate.HasValue &&
                    lot.ExpiryDate.Value < today)
                {
                    continue;
                }

                if (!balances.TryGetValue(lot.Id, out var balance) ||
                    balance.Quantity <= 0)
                {
                    continue;
                }

                candidates.Add(new AvailableLot(
                    lot.Id,
                    balance.Quantity,
                    balance.FirstReceivedAtUtc,
                    lot.ExpiryDate));
            }

            var availableQuantity = candidates.Sum(x => x.Quantity);

            if (availableQuantity < input.Quantity)
            {
                throw Invalid(
                    nameof(input.Quantity),
                    $"Kaynak depoda transfere uygun stok yetersiz. " +
                    $"Kullanılabilir miktar: " +
                    $"{availableQuantity:0.###} {product.Unit}.");
            }

            IOrderedEnumerable<AvailableLot> orderedLots =
                product.IssueMethod switch
                {
                    StockIssueMethod.Fifo => candidates
                        .OrderBy(x => x.FirstReceivedAtUtc)
                        .ThenBy(x => x.LotId),

                    StockIssueMethod.Fefo => candidates
                        .OrderBy(x => x.ExpiryDate ?? DateOnly.MaxValue)
                        .ThenBy(x => x.FirstReceivedAtUtc)
                        .ThenBy(x => x.LotId),

                    _ => throw Invalid(
                        nameof(input.ProductId),
                        "Ürünün çıkış yöntemi geçersiz.")
                };

            await _operations.AddAsync(new StockOperation
            {
                Id = input.OperationId,
                Type = "Transfer",
                RequestHash = requestHash,
                UserId = userId,
                CreatedAtUtc = now
            }, ct);

            var remaining = input.Quantity;

            foreach (var lot in orderedLots)
            {
                if (remaining == 0)
                {
                    break;
                }

                var take = Math.Min(remaining, lot.Quantity);

                await _movements.AddAsync(new StockMovement
                {
                    OperationId = input.OperationId,
                    WarehouseId = input.SourceWarehouseId,
                    LotId = lot.LotId,
                    Type = StockMovementType.TransferOut,
                    Quantity = -take,
                    Description = input.Description,
                    PerformedByUserId = userId,
                    CreatedAtUtc = now
                }, ct);

                await _movements.AddAsync(new StockMovement
                {
                    OperationId = input.OperationId,
                    WarehouseId = input.TargetWarehouseId,
                    LotId = lot.LotId,
                    Type = StockMovementType.TransferIn,
                    Quantity = take,
                    Description = input.Description,
                    PerformedByUserId = userId,
                    CreatedAtUtc = now
                }, ct);

                remaining -= take;
            }

        }, cancellationToken);

        return input.OperationId;
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

    private sealed record AvailableLot(
        int LotId,
        decimal Quantity,
        DateTime FirstReceivedAtUtc,
        DateOnly? ExpiryDate);
}