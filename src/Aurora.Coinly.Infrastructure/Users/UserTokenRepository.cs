using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Infrastructure.Users;

internal sealed class UserTokenRepository(
    ApplicationDbContext dbContext) : BaseRepository<UserToken>(dbContext), IUserTokenRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public new async Task AddAsync(UserToken userToken, CancellationToken cancellationToken)
    {
        await dbContext
            .UserTokens
            .Where(x => x.UserId == userToken.UserId && x.IsActive)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsActive, false), cancellationToken);

        await dbContext.Set<UserToken>().AddAsync(userToken, cancellationToken);
    }
}