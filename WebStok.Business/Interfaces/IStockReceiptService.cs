using WebStok.Business.DTOs;

namespace WebStok.Business.Interfaces;

public interface IStockReceiptService
{
    Task<StockReceiptPageDataDto> GetPageDataAsync(
        CancellationToken cancellationToken = default);

    Task<Guid> ReceiveAsync(
        StockReceiptDto dto,
        CancellationToken cancellationToken = default);
}