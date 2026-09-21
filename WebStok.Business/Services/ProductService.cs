using FluentValidation;
using FluentValidation.Results;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Domain.Entities;
using WebStok.Domain.Interfaces;

namespace WebStok.Business.Services;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _products;
    private readonly IRepository<Category> _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateProductDto> _validator;

    public ProductService(
        IRepository<Product> products,
        IRepository<Category> categories,
        IUnitOfWork unitOfWork,
        IValidator<CreateProductDto> validator)
    {
        _products = products;
        _categories = categories;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<List<ProductListDto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await _products.ListAsync(
            cancellationToken: cancellationToken);

        var categories = await _categories.ListAsync(
            cancellationToken: cancellationToken);

        var lookup = categories.ToDictionary(x => x.Id);

        return products
            .OrderBy(x => x.Code)
            .Select(x => new ProductListDto
            {
                Id = x.Id,
                Code = x.Code,
                Barcode = x.Barcode,
                Name = x.Name,
                CategoryPath = BuildCategoryPath(x.CategoryId, lookup),
                Unit = x.Unit,
                PurchasePrice = x.PurchasePrice,
                SalePrice = x.SalePrice,
                MinimumStockLevel = x.MinimumStockLevel,
                IssueMethod = x.IssueMethod,
                IsActive = x.IsActive
            })
            .ToList();
    }

    public async Task<int> CreateAsync(
        CreateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var input = new CreateProductDto
        {
            Code = dto.Code?.Trim().ToUpperInvariant() ?? string.Empty,
            Barcode = dto.Barcode?.Trim().ToUpperInvariant() ?? string.Empty,
            Name = dto.Name?.Trim() ?? string.Empty,
            CategoryId = dto.CategoryId,
            Unit = dto.Unit?.Trim() ?? string.Empty,
            PurchasePrice = dto.PurchasePrice,
            SalePrice = dto.SalePrice,
            MinimumStockLevel = dto.MinimumStockLevel,
            IssueMethod = dto.IssueMethod
        };

        await _validator.ValidateAndThrowAsync(
            input, cancellationToken);

        var failures = new List<ValidationFailure>();

        var category = await _categories.GetByIdAsync(
            input.CategoryId, cancellationToken);

        if (category is null || !category.IsActive)
        {
            failures.Add(new ValidationFailure(
                nameof(CreateProductDto.CategoryId),
                "Kategori bulunamadı veya aktif değil."));
        }

        var existing = await _products.ListAsync(
            x => x.Code == input.Code || x.Barcode == input.Barcode,
            cancellationToken);

        if (existing.Any(x => x.Code == input.Code))
        {
            failures.Add(new ValidationFailure(
                nameof(CreateProductDto.Code),
                "Bu ürün kodu zaten kullanılıyor."));
        }

        if (existing.Any(x => x.Barcode == input.Barcode))
        {
            failures.Add(new ValidationFailure(
                nameof(CreateProductDto.Barcode),
                "Bu barkod zaten kullanılıyor."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        var product = new Product
        {
            Code = input.Code,
            Barcode = input.Barcode,
            Name = input.Name,
            CategoryId = input.CategoryId,
            Unit = input.Unit,
            PurchasePrice = input.PurchasePrice,
            SalePrice = input.SalePrice,
            MinimumStockLevel = input.MinimumStockLevel,
            IssueMethod = input.IssueMethod,
            IsActive = true
        };

        await _products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }

        private static string BuildCategoryPath(
        int categoryId,
        IReadOnlyDictionary<int, Category> categories)
    {
        var names = new Stack<string>();
        var visited = new HashSet<int>();
        int? currentId = categoryId;

        while (currentId.HasValue &&
               categories.TryGetValue(currentId.Value, out var category))
        {
            if (!visited.Add(category.Id))
            {
                names.Push("[Geçersiz kategori bağlantısı]");
                break;
            }

            names.Push(category.Name);
            currentId = category.ParentId;
        }

        return string.Join(" / ", names);
    }
}