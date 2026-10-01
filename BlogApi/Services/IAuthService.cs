using BlogApi.Dtos.Auth;
using BlogApi.Dtos.Users;
using Microsoft.AspNetCore.Identity;

namespace BlogApi.Services;

public interface IAuthService
{
    // Result tells whether Identity accepted the user. User is set only when it did.
    Task<(IdentityResult Result, UserResponseDto? User)> RegisterAsync(RegisterDto dto);
}
