namespace Aurora.Coinly.Application.Abstractions.Data;

public interface ICoinlyDbContextFactory
{
    Task<ICoinlyDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
}