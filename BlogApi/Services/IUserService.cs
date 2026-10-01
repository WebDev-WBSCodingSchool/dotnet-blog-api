using BlogApi.Dtos.Users;

namespace BlogApi.Services;

// Read-only access to users. New users are created through IAuthService.RegisterAsync.
public interface IUserService
{
    Task<IReadOnlyList<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
