using BlogApi.Data;
using BlogApi.Dtos.Users;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;

    public UserService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetAllAsync()
    {
        // AsNoTracking: we only read the data, so EF Core does not need to watch it for changes.
        // Select: only the columns the DTO needs are loaded.
        // Email is nullable on IdentityUser, but registration always sets it, hence the "!".
        return await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserResponseDto(u.Id, u.Name, u.Email!, u.CreatedAt))
            .ToListAsync();
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        return await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto(u.Id, u.Name, u.Email!, u.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _db.Users.AnyAsync(u => u.Id == id);
    }
}
