using System.Linq.Expressions;

namespace WebStok.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(
        object id,
        CancellationToken cancellationToken = default);

    Task<List<T>> ListAsync(
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default);
}

