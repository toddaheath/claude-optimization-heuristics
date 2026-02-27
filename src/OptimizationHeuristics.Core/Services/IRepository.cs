using System.Linq.Expressions;

namespace OptimizationHeuristics.Core.Services;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> FindPagedAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, int page, int pageSize, bool descending = false);
    Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
}
