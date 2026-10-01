namespace BlogApi.Models;

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset PublishedAt { get; set; }

    // Foreign key to the author.
    public Guid UserId { get; set; }

    // Navigation property to the author. EF Core fills it when the query asks for it,
    // so it starts as null! to tell the compiler it is set before use.
    public User User { get; set; } = null!;
}
