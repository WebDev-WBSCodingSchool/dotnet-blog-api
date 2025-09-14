using BlogApi.Models;

namespace BlogApi.Services;

public class InMemoryPostService : IPostService
{
    private readonly Dictionary<Guid, Post> _posts = new();
    private readonly IUserService _userService;

    public InMemoryPostService(IUserService userService)
    {
        _userService = userService;
    }

    public Task<Post?> GetAsync(Guid id)
    {
        _posts.TryGetValue(id, out var post);
        return Task.FromResult(post);
    }

    public Task<IReadOnlyList<Post>> ListAsync()
    {
        return Task.FromResult<IReadOnlyList<Post>>(_posts.Values.ToList());
    }

    public Task<IReadOnlyList<Post>> ListByUserAsync(Guid userId)
    {
        var userPosts = _posts.Values.Where(p => p.UserId == userId).ToList();
        return Task.FromResult<IReadOnlyList<Post>>(userPosts);
    }

    public async Task<Post> CreateAsync(Guid userId, string title, string content)
    {
        // Validate that the user exists
        var user = await _userService.GetAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Content = content,
            PublishedAt = DateTimeOffset.UtcNow
        };

        _posts[post.Id] = post;
        return post;
    }

    public Task<Post?> UpdateAsync(Guid id, string? title, string? content)
    {
        if (!_posts.TryGetValue(id, out var post))
            return Task.FromResult<Post?>(null);

        if (title != null)
            post.Title = title;

        if (content != null)
            post.Content = content;

        return Task.FromResult<Post?>(post);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_posts.Remove(id));
    }
}