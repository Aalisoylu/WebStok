using WebStok.Business.DTOs;

namespace WebStok.Business.Interfaces;

public interface IStockMovementService
{
    Task<List<StockMovementListDto>> ListAsync(
        CancellationToken cancellationToken = default);
}