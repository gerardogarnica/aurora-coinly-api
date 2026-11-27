using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Aurora.Coinly.Infrastructure.Authentication;

internal sealed class TokenProvider(
    IOptions<JwtAuthOptions> jwtAuthOptions,
    IDateTimeService dateTimeService) : ITokenProvider
{
    private const int RefreshTokenByteLength = 64;

    private readonly JwtAuthOptions _jwtAuthOptions = jwtAuthOptions.Value;
    private readonly IDateTimeService _dateTimeService = dateTimeService;

    public IssuedToken CreateToken(TokenRequest tokenRequest)
    {
        DateTime utcNow = _dateTimeService.UtcNow;
        string accessTokenId = Guid.NewGuid().ToString("N");

        (string accessToken, DateTimeOffset accessTokenExpiresOn) = GenerateAccessToken(tokenRequest, accessTokenId, utcNow);
        (string refreshToken, DateTimeOffset refreshTokenExpiresOn) = GenerateRefreshToken(utcNow);

        var identityToken = new IdentityToken(
            accessToken,
            accessTokenExpiresOn,
            refreshToken,
            refreshTokenExpiresOn);

        return new IssuedToken(identityToken, accessTokenId, HashRefreshToken(refreshToken));
    }

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    private (string Token, DateTimeOffset ExpiresOn) GenerateAccessToken(TokenRequest tokenRequest, string accessTokenId, DateTime utcNow)
    {
        DateTime expiresOn = utcNow.AddMinutes(_jwtAuthOptions.LifeTimeInMinutes);

        // Create security key from the JWT key
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtAuthOptions.Key));

        // Create signing credentials using the security key and HMAC SHA256 algorithm
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        string userId = tokenRequest.UserId.ToString();
        string fullName = $"{tokenRequest.FirstName} {tokenRequest.LastName}";

        // Set list of claims
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Sid, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(JwtRegisteredClaimNames.Email, tokenRequest.Email),
            new(JwtRegisteredClaimNames.GivenName, tokenRequest.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, tokenRequest.LastName),
            new(JwtRegisteredClaimNames.Name, fullName),
            new(JwtRegisteredClaimNames.Jti, accessTokenId),
            new(JwtRegisteredClaimNames.Typ, "Bearer"),
            new(
                JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(utcNow).ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            .. tokenRequest.Roles.Select(role => new Claim(ClaimTypes.Role, role)),
        ];

        // Create the JWT security token
        JwtSecurityToken token = new(
            issuer: _jwtAuthOptions.Issuer,
            audience: _jwtAuthOptions.Audience,
            claims: claims,
            notBefore: utcNow,
            expires: expiresOn,
            signingCredentials: signingCredentials);

        // Generate the token
        string serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

        return (serializedToken, new DateTimeOffset(expiresOn, TimeSpan.Zero));
    }

    private (string Token, DateTimeOffset ExpiresOn) GenerateRefreshToken(DateTime utcNow)
    {
        Span<byte> bytes = stackalloc byte[RefreshTokenByteLength];
        RandomNumberGenerator.Fill(bytes);

        string token = Convert.ToBase64String(bytes);
        DateTime expiresOn = utcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays);

        return (token, new DateTimeOffset(expiresOn, TimeSpan.Zero));
    }
}