using Aurora.Coinly.Application.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Aurora.Coinly.Infrastructure.Authentication;

internal sealed class TokenProvider(IOptions<JwtAuthOptions> options) : ITokenProvider
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

    public IdentityToken CreateToken(TokenRequest tokenRequest)
    {
        var accessToken = CreateAccessToken(tokenRequest);
        var refreshToken = CreateRefreshToken();

        return new IdentityToken(accessToken, refreshToken);
    }

    private string CreateAccessToken(TokenRequest tokenRequest)
    {
        // Create security key from the JWT key
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAuthOptions.Key));

        // Create signing credentials using the security key and HMAC SHA256 algorithm
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Set list of claims
        List<Claim> claims =
        [
            new Claim(CustomClaims.Sub, tokenRequest.UserId),
            new Claim(JwtRegisteredClaimNames.Sid, tokenRequest.UserId),
            new Claim(JwtRegisteredClaimNames.Email, tokenRequest.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, tokenRequest.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, tokenRequest.LastName),
            new Claim(JwtRegisteredClaimNames.Name, $"{tokenRequest.FirstName} {tokenRequest.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Typ, "Bearer"),
            ..tokenRequest.Roles.Select(role => new Claim(ClaimTypes.Role, role))
        ];

        // Create the security token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtAuthOptions.Issuer,
            Audience = _jwtAuthOptions.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_jwtAuthOptions.LifeTimeInMinutes),
            SigningCredentials = signingCredentials,
        };

        // Create the token handler and generate the token
        var tokenHandler = new JsonWebTokenHandler();
        var accessToken = tokenHandler.CreateToken(tokenDescriptor);

        return accessToken;
    }

    private string CreateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(randomBytes);
    }
}