namespace Aurora.Coinly.Domain.Users;

public interface IUserTokenRepository : IRepository<UserToken>
{
    Task<UserToken?> GetByRefreshTokenAsync(string refreshToken);
}