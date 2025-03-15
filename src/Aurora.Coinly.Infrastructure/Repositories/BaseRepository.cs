namespace Aurora.Coinly.Infrastructure.Repositories;

internal abstract class BaseRepository<T>(DbContext dbContext)
    where T : BaseEntity
{
    private readonly DbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }
}