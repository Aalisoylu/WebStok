using WebStok.Business.DTOs;

namespace WebStok.Business.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryListDto>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        CreateCategoryDto dto,
        CancellationToken cancellationToken = default);
}