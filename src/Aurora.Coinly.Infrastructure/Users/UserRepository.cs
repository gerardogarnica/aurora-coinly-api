using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Infrastructure.Users;

internal sealed class UserRepository(
    ApplicationDbContext dbContext) : BaseRepository<User>(dbContext), IUserRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public new async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        foreach (Role role in user.Roles)
        {
            dbContext.Attach(role);
        }

        await dbContext.Set<User>().AddAsync(user, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email) => dbContext
        .Users
        .Include(x => x.Roles)
        .FirstOrDefaultAsync(x => x.Email == email);

    public Task<User?> GetByIdAsync(Guid id) => dbContext
        .Users
        .Include(x => x.Roles)
        .FirstOrDefaultAsync(x => x.Id == id);
}