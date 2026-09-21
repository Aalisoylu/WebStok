using FluentValidation;
using FluentValidation.Results;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Domain.Entities;
using WebStok.Domain.Interfaces;

namespace WebStok.Business.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateCategoryDto> _validator;

    public CategoryService(
        IRepository<Category> repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateCategoryDto> validator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<List<CategoryListDto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _repository.ListAsync(
            cancellationToken: cancellationToken);

        return categories
            .OrderBy(x => x.Name)
            .Select(x => new CategoryListDto
            {
                Id = x.Id,
                Name = x.Name,
                ParentId = x.ParentId,
                IsActive = x.IsActive
            })
            .ToList();
    }

    public async Task<int> CreateAsync(
        CreateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var input = new CreateCategoryDto
        {
            Name = dto.Name?.Trim() ?? string.Empty,
            ParentId = dto.ParentId
        };

        await _validator.ValidateAndThrowAsync(
            input, cancellationToken);

        if (input.ParentId.HasValue)
        {
            var parent = await _repository.GetByIdAsync(
                input.ParentId.Value, cancellationToken);

            if (parent is null || !parent.IsActive)
            {
                throw new ValidationException(
                    new[]
                    {
                        new ValidationFailure(
                            nameof(CreateCategoryDto.ParentId),
                            "Üst kategori bulunamadı veya aktif değil.")
                    });
            }
        }

        var category = new Category
        {
            Name = input.Name,
            ParentId = input.ParentId,
            IsActive = true
        };

        await _repository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}