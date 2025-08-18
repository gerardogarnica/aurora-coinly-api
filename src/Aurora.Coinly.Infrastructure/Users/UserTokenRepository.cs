using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Infrastructure.Users;

internal sealed class UserTokenRepository(
    ApplicationDbContext dbContext) : BaseRepository<UserToken>(dbContext), IUserTokenRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public Task<UserToken?> GetByRefreshTokenAsync(string refreshToken) => dbContext
        .UserTokens
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
}