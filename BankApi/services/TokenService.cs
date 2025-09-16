namespace BankApi.Services;
using BankApi.Entities;
using BankApi.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenService(IConfiguration config)
    {
        _secret = config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key is missing");
        _issuer = config["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer is missing");
        _audience = config["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience is missing");
    }

    public string GenerateToken(Client client)
    {
        var claims = new[]
        {
            new Claim("id", client.Id.ToString()),
            new Claim("role", client.Role.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = creds,
            Issuer = _issuer,
            Audience = _audience
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
