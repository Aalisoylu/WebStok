using WebStok.Business.DTOs;

namespace WebStok.Business.Interfaces;

public interface IStockIssueService
{
    Task<Guid> IssueAsync(
        StockIssueDto dto,
        CancellationToken cancellationToken = default);
}