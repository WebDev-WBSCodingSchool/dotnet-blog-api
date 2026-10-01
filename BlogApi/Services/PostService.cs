using BlogApi.Data;
using BlogApi.Dtos.Posts;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PostService> _logger;
    private readonly BlogMetrics _metrics;

    public PostService(ApplicationDbContext db, ILogger<PostService> logger, BlogMetrics metrics)
    {
        _db = db;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<IReadOnlyList<PostResponseDto>> GetAllAsync(string? search)
    {
        // Build the query step by step. Nothing runs against the database until ToListAsync.
        var query = _db.Posts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
        }

        // Reading p.User.Name inside Select makes EF Core add a join to the users table.
        // Include is only needed when a query returns entities, so it is left out here.
        return await query
            .OrderByDescending(p => p.PublishedAt)
            .Select(p => new PostResponseDto(p.Id, p.UserId, p.User.Name, p.Title, p.Content, p.PublishedAt))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PostResponseDto>> GetByUserAsync(Guid userId)
    {
        return await _db.Posts
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PublishedAt)
            .Select(p => new PostResponseDto(p.Id, p.UserId, p.User.Name, p.Title, p.Content, p.PublishedAt))
            .ToListAsync();
    }

    public async Task<PostResponseDto?> GetByIdAsync(Guid id)
    {
        return await _db.Posts
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PostResponseDto(p.Id, p.UserId, p.User.Name, p.Title, p.Content, p.PublishedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<PostResponseDto?> CreateAsync(Guid userId, CreatePostDto dto)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Cannot create a post: user {UserId} does not exist", userId);
            return null;
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            User = user,
            Title = dto.Title,
            Content = dto.Content,
            PublishedAt = DateTimeOffset.UtcNow
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        // {PostId} and {UserId} are stored as separate properties, so the logs can be searched by them.
        _logger.LogInformation("User {UserId} created post {PostId}", userId, post.Id);
        _metrics.PostCreated();

        return ToDto(post);
    }

    public async Task<PostResponseDto?> UpdateAsync(Guid id, UpdatePostDto dto)
    {
        // Include loads the author together with the post, so ToDto can read post.User.Name.
        // The query is tracked, so SaveChangesAsync writes the changes back.
        var post = await _db.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null)
        {
            return null;
        }

        post.Title = dto.Title;
        post.Content = dto.Content;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Post {PostId} updated", id);

        return ToDto(post);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post is null)
        {
            return false;
        }

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Post {PostId} deleted", id);
        return true;
    }

    // Expects post.User to be loaded.
    private static PostResponseDto ToDto(Post post) =>
        new(post.Id, post.UserId, post.User.Name, post.Title, post.Content, post.PublishedAt);
}
