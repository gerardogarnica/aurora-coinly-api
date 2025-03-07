namespace Aurora.Coinly.Domain.Abstractions;

public interface IRepository<in T> where T : BaseEntity
{
    IUnitOfWork UnitOfWork { get; }

    Task AddAsync(T entity);
    void Update(T entity);
}