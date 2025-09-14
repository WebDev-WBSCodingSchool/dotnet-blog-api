using BlogApi.Models;

namespace BlogApi.Services;

public class InMemoryUserService : IUserService
{
    private readonly Dictionary<Guid, User> _users = new();

    public Task<User?> GetAsync(Guid id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<User>> ListAsync()
    {
        return Task.FromResult<IReadOnlyList<User>>(_users.Values.ToList());
    }

    public Task<User> CreateAsync(string name, string email)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _users[user.Id] = user;
        return Task.FromResult(user);
    }

    public Task<User?> UpdateAsync(Guid id, string? name, string? email)
    {
        if (!_users.TryGetValue(id, out var user))
            return Task.FromResult<User?>(null);

        if (name != null)
            user.Name = name;

        if (email != null)
            user.Email = email;

        return Task.FromResult<User?>(user);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_users.Remove(id));
    }
}