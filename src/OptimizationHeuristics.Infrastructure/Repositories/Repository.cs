using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OptimizationHeuristics.Core.Services;
using OptimizationHeuristics.Infrastructure.Data;

namespace OptimizationHeuristics.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync() => await DbSet.ToListAsync();

    public async Task<T?> GetByIdAsync(Guid id) => await DbSet.FindAsync(id);

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await DbSet.Where(predicate).ToListAsync();

    public async Task<List<T>> FindPagedAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, int page, int pageSize, bool descending = false)
    {
        var query = DbSet.Where(predicate);
        query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate)
        => await DbSet.FirstOrDefaultAsync(predicate);

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        => await DbSet.CountAsync(predicate);

    public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    public void Update(T entity) => DbSet.Update(entity);

    public void Delete(T entity) => DbSet.Remove(entity);
}
