namespace Aurora.Coinly.Application.Users.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserProfileResponse>;
