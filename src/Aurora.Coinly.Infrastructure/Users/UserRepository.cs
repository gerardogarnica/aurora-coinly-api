using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Infrastructure.Users;

internal sealed class UserRepository(
    ApplicationDbContext dbContext) : BaseRepository<User>(dbContext), IUserRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public Task<User?> GetByEmailAsync(string email) => dbContext
        .Users
        .FirstOrDefaultAsync(x => x.Email == email);

    public Task<User?> GetByIdAsync(Guid id) => dbContext
        .Users
        .FirstOrDefaultAsync(x => x.Id == id);
}