using Microsoft.AspNetCore.Identity;

namespace BlogApi.Models;

// IdentityUser<Guid> already has Id, UserName, Email, PasswordHash and more.
// The <Guid> makes the ids Guids. The default key type is string.
public class User : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation property: the posts written by this user.
    public List<Post> Posts { get; set; } = [];
}
