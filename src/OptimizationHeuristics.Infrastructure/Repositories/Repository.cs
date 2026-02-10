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

    public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    public void Update(T entity) => DbSet.Update(entity);

    public void Delete(T entity) => DbSet.Remove(entity);
}
