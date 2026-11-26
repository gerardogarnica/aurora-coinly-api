namespace Aurora.Coinly.Application.Abstractions.Authentication;

public sealed record TokenRequest(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles);
