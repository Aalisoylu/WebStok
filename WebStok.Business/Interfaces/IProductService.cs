using WebStok.Business.DTOs;

namespace WebStok.Business.Interfaces;

public interface IProductService
{
    Task<List<ProductListDto>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        CreateProductDto dto,
        CancellationToken cancellationToken = default);
}