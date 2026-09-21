using WebStok.Business.DTOs;

namespace WebStok.Business.Interfaces;

public interface IStockTransferService
{
    Task<Guid> TransferAsync(
        StockTransferDto dto,
        CancellationToken cancellationToken = default);
}