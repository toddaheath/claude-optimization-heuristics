namespace OptimizationHeuristics.Core.Services;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
}
