namespace SmartInventory.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public interface IJwtService
{
    string GenerateToken(int userId, string username, string role, int expirationMinutes = 60);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(string secretKey, string issuer = "SmartInventoryAPI", string audience = "SmartInventoryAPIUsers")
    {
        _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerateToken(int userId, string username, string role, int expirationMinutes = 60)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, username),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role),
            new System.Security.Claims.Claim("sub", userId.ToString()),
            new System.Security.Claims.Claim("username", username),
            new System.Security.Claims.Claim("role", role),
            new System.Security.Claims.Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
