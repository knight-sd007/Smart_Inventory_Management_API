namespace SmartInventory.Infrastructure.Services;

using SmartInventory.Application.DTOs.Auth;
using SmartInventory.Application.Interfaces;
using SmartInventory.Domain.Entities;
using Microsoft.Extensions.Configuration;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly int _expiryMinutes;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IConfiguration configuration)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _expiryMinutes = int.TryParse(configuration["JwtSettings:ExpiryMinutes"], out var minutes) ? minutes : 60;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null || !_jwtService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid username or password");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("User account is inactive");
        }

        var token = _jwtService.GenerateToken(user.Id, user.Username, user.Role, _expiryMinutes);
        user.LastLogin = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_expiryMinutes)
        };
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already exists");
        }

        var passwordHash = _jwtService.HashPassword(request.Password);
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            Role = "User",
            IsActive = true
        };

        user = await _userRepository.AddAsync(user);

        return new RegisterResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Message = "User registered successfully. Please login to continue."
        };
    }
}