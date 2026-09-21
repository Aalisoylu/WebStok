using FluentValidation;
using FluentValidation.Results;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Domain.Entities;
using WebStok.Domain.Interfaces;

namespace WebStok.Business.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IRepository<Warehouse> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateWarehouseDto> _validator;

    public WarehouseService(
        IRepository<Warehouse> repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateWarehouseDto> validator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<List<WarehouseListDto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var warehouses = await _repository.ListAsync(
            cancellationToken: cancellationToken);

        return warehouses
            .OrderBy(x => x.Code)
            .Select(x => new WarehouseListDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToList();
    }

    public async Task<int> CreateAsync(
        CreateWarehouseDto dto,
        CancellationToken cancellationToken = default)
    {
        var input = new CreateWarehouseDto
        {
            Code = dto.Code?.Trim().ToUpperInvariant() ?? string.Empty,
            Name = dto.Name?.Trim() ?? string.Empty,
            Description = dto.Description?.Trim()
        };

        await _validator.ValidateAndThrowAsync(
            input, cancellationToken);

        var existing = await _repository.ListAsync(
            x => x.Code == input.Code, cancellationToken);

        if (existing.Count > 0)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    nameof(CreateWarehouseDto.Code),
                    "Bu depo kodu zaten kullanılıyor.")
            });
        }

        var warehouse = new Warehouse
        {
            Code = input.Code,
            Name = input.Name,
            Description = input.Description,
            IsActive = true
        };

        await _repository.AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return warehouse.Id;
    }
}