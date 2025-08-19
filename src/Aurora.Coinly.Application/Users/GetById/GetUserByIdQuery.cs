namespace Aurora.Coinly.Application.Users.GetById;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserModel>;