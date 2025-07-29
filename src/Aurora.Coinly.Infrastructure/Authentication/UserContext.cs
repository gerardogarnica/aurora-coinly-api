using Microsoft.AspNetCore.Http;

namespace Aurora.Coinly.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor contextAccessor) : IUserContext
{
    public Guid UserId => contextAccessor.HttpContext?.User.GetUserId()
        ?? throw new AuroraCoinlyException("User context is not available. Ensure that the HTTP context is set up correctly.");
}