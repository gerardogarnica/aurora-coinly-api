using Aurora.Coinly.Application.Abstractions.Data;

namespace Aurora.Coinly.Infrastructure.Database;

internal sealed class CoinlyDbContextFactory(
    IDbContextFactory<ApplicationDbContext> dbContextFactory) : ICoinlyDbContextFactory
{
    public async Task<ICoinlyDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return await dbContextFactory.CreateDbContextAsync(cancellationToken);
    }
}