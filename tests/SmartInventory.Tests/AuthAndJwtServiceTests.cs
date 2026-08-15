namespace SmartInventory.Tests;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using SmartInventory.Infrastructure.Services;
using Xunit;

public class AuthAndJwtServiceTests
{
    private const string SecretKey = "test-super-secret-key-32-chars-long-minimum!";
    private readonly JwtService _jwtService;

    public AuthAndJwtServiceTests()
    {
        _jwtService = new JwtService(SecretKey, "TestIssuer", "TestAudience");
    }

    [Fact]
    public void HashPassword_ProducesConsistentHash()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash1 = _jwtService.HashPassword(password);
        var hash2 = _jwtService.HashPassword(password);

        // Assert
        Assert.NotNull(hash1);
        Assert.NotEmpty(hash1);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrue_ForCorrectPassword()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _jwtService.HashPassword(password);

        // Act
        var isVerified = _jwtService.VerifyPassword(password, hash);

        // Assert
        Assert.True(isVerified);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForWrongPassword()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _jwtService.HashPassword(password);

        // Act
        var isVerified = _jwtService.VerifyPassword("WrongPassword123!", hash);

        // Assert
        Assert.False(isVerified);
    }

    [Fact]
    public void GenerateToken_ProducesValidJwtWithExpectedClaims()
    {
        // Act
        var tokenString = _jwtService.GenerateToken(userId: 42, username: "admin_user", role: "Admin", expirationMinutes: 30);

        // Assert
        Assert.NotNull(tokenString);
        Assert.NotEmpty(tokenString);

        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(tokenString));

        var jwtToken = handler.ReadJwtToken(tokenString);
        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Contains("TestAudience", jwtToken.Audiences);

        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        var userClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        Assert.Equal("42", subClaim);
        Assert.Equal("admin_user", userClaim);
        Assert.Equal("Admin", roleClaim);
    }
}
