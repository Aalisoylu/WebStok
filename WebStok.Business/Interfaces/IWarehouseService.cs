using WebStok.Business.DTOs;

namespace WebStok.Business.Interfaces;

public interface IWarehouseService
{
    Task<List<WarehouseListDto>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        CreateWarehouseDto dto,
        CancellationToken cancellationToken = default);
}