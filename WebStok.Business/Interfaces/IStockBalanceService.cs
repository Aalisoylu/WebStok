using WebStok.Business.DTOs;

namespace WebStok.Business.Interfaces;

public interface IStockBalanceService
{
    Task<List<StockBalanceDto>> ListAsync(
        CancellationToken cancellationToken = default);
}