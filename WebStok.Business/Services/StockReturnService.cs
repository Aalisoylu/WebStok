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

public class StockReturnService : IStockReturnService
{
    private readonly IRepository<Product> _products;
    private readonly IRepository<Warehouse> _warehouses;
    private readonly IRepository<Lot> _lots;
    private readonly IRepository<StockMovement> _movements;
    private readonly IRepository<StockOperation> _operations;
    private readonly IRepository<UserWarehouse> _warehouseAccess;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<StockReturnDto> _validator;

    public StockReturnService(
        IRepository<Product> products,
        IRepository<Warehouse> warehouses,
        IRepository<Lot> lots,
        IRepository<StockMovement> movements,
        IRepository<StockOperation> operations,
        IRepository<UserWarehouse> warehouseAccess,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IValidator<StockReturnDto> validator)
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

    public async Task<Guid> ReturnAsync(
        StockReturnDto dto,
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
                "İade kaydetme yetkiniz yok.");
        }

        var input = new StockReturnDto
        {
            OperationId = dto.OperationId,
            OriginalMovementId = dto.OriginalMovementId,
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
            var original = await _movements.GetByIdAsync(
                input.OriginalMovementId, ct);

            if (original is null)
            {
                throw Invalid(
                    nameof(input.OriginalMovementId),
                    "Çıkış hareketi bulunamadı.");
            }

            if (role == UserRole.WarehouseSupervisor)
            {
                var permissions = await _warehouseAccess.ListAsync(
                    x => x.UserId == userId &&
                         x.WarehouseId == original.WarehouseId,
                    ct);

                if (permissions.Count == 0)
                {
                    throw new AccessDeniedException(
                        "Bu depoda iade işlemi yapma yetkiniz yok.");
                }
            }

            var existingOperation = await _operations.GetByIdAsync(
                input.OperationId, ct);

            if (existingOperation is not null)
            {
                if (existingOperation.UserId != userId ||
                    existingOperation.Type != "Return" ||
                    existingOperation.RequestHash != requestHash)
                {
                    throw Invalid(
                        nameof(input.OperationId),
                        "İşlem kimliği başka bir işlemde kullanılmış. " +
                        "Yeni bir iade için sayfayı yeniden açınız.");
                }

                // Aynı iade zaten kaydedilmiş; tekrar stok artırma.
                return;
            }

            if (original.Type is not (
                    StockMovementType.ProductionIssue or
                    StockMovementType.ServiceIssue) ||
                original.Quantity >= 0)
            {
                throw Invalid(
                    nameof(input.OriginalMovementId),
                    "Yalnızca üretim veya servis çıkışına iade yapılabilir.");
            }

            var warehouse = await _warehouses.GetByIdAsync(
                original.WarehouseId, ct);

            if (warehouse is null || !warehouse.IsActive)
            {
                throw Invalid(
                    nameof(input.OriginalMovementId),
                    "İadenin yapılacağı depo aktif değil.");
            }

            var lot = await _lots.GetByIdAsync(original.LotId, ct);

            if (lot is null)
            {
                throw Invalid(
                    nameof(input.OriginalMovementId),
                    "Çıkışa ait lot bulunamadı.");
            }

            var product = await _products.GetByIdAsync(
                lot.ProductId, ct);

            if (product is null || !product.IsActive)
            {
                throw Invalid(
                    nameof(input.OriginalMovementId),
                    "İade edilecek ürün aktif değil.");
            }

            if (product.Unit == "Adet" &&
                input.Quantity != decimal.Truncate(input.Quantity))
            {
                throw Invalid(
                    nameof(input.Quantity),
                    "Adet birimli ürünlerde miktar tam sayı olmalıdır.");
            }

            var previousReturns = await _movements.ListAsync(
                x => x.RelatedMovementId == original.Id &&
                     x.Type == StockMovementType.ReturnIn,
                ct);

            var returnedQuantity = previousReturns.Sum(x => x.Quantity);
            var issuedQuantity = -original.Quantity;
            var remainingReturnable = issuedQuantity - returnedQuantity;

            if (input.Quantity > remainingReturnable)
            {
                throw Invalid(
                    nameof(input.Quantity),
                    $"İade miktarı kalan hakkı aşıyor. " +
                    $"İade edilebilir miktar: " +
                    $"{remainingReturnable:0.###} {product.Unit}.");
            }

            var now = DateTime.UtcNow;

            await _operations.AddAsync(new StockOperation
            {
                Id = input.OperationId,
                Type = "Return",
                RequestHash = requestHash,
                UserId = userId,
                CreatedAtUtc = now
            }, ct);

            await _movements.AddAsync(new StockMovement
            {
                OperationId = input.OperationId,
                RelatedMovementId = original.Id,
                WarehouseId = original.WarehouseId,
                LotId = original.LotId,
                Type = StockMovementType.ReturnIn,
                Quantity = input.Quantity,
                Description = input.Description,
                PerformedByUserId = userId,
                CreatedAtUtc = now
            }, ct);

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
}