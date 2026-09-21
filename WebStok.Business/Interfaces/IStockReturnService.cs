using WebStok.Business.DTOs;

namespace WebStok.Business.Interfaces;

public interface IStockReturnService
{
    Task<Guid> ReturnAsync(
        StockReturnDto dto,
        CancellationToken cancellationToken = default);
}