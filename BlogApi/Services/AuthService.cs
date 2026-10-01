using System.Security.Claims;
using System.Text;
using BlogApi.Dtos.Auth;
using BlogApi.Dtos.Users;
using BlogApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<User> userManager, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(IdentityResult Result, UserResponseDto? User)> RegisterAsync(RegisterDto dto)
    {
        var user = new User
        {
            Name = dto.Name,
            UserName = dto.Email,
            Email = dto.Email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // UserManager checks the password rules, hashes the password and saves the user.
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            _logger.LogInformation("Registration rejected: {Errors}", string.Join(", ", result.Errors.Select(e => e.Code)));
            return (result, null);
        }

        _logger.LogInformation("User {UserId} registered", user.Id);

        return (result, new UserResponseDto(user.Id, user.Name, user.Email, user.CreatedAt));
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            // Same answer for an unknown email and a wrong password,
            // so callers cannot find out which emails are registered.
            // The email is not logged, because logs should not contain personal data.
            _logger.LogWarning("Failed login attempt");
            return null;
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpiryMinutes", 60));
        var token = CreateToken(user, expiresAt);
        _logger.LogInformation("User {UserId} logged in", user.Id);

        return new LoginResponseDto(token, expiresAt);
    }

    private string CreateToken(User user, DateTimeOffset expiresAt)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var descriptor = new SecurityTokenDescriptor
        {
            // "sub" (subject) holds the user id. The endpoints read it to find the current user.
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Name, user.Name)
            ]),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
