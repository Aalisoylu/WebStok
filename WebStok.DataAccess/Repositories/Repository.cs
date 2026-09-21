using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebStok.DataAccess.Persistence;
using WebStok.Domain.Interfaces;

namespace WebStok.DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly WebStokDbContext _context;

    public Repository(WebStokDbContext context)
    {
        _context = context;
    }

    //Listeleme işlemleri
    public async Task<T?> GetByIdAsync(
        object id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>()
            .FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<List<T>> ListAsync(
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _context.Set<T>().AsNoTracking();

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
    }
}