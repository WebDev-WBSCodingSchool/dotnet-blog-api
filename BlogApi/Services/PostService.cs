using BlogApi.Data;
using BlogApi.Dtos.Posts;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _db;

    public PostService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PostResponseDto>> GetAllAsync()
    {
        return await _db.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.PublishedAt)
            .Select(p => new PostResponseDto(p.Id, p.UserId, p.Title, p.Content, p.PublishedAt))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PostResponseDto>> GetByUserAsync(Guid userId)
    {
        return await _db.Posts
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PublishedAt)
            .Select(p => new PostResponseDto(p.Id, p.UserId, p.Title, p.Content, p.PublishedAt))
            .ToListAsync();
    }

    public async Task<PostResponseDto?> GetByIdAsync(Guid id)
    {
        return await _db.Posts
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PostResponseDto(p.Id, p.UserId, p.Title, p.Content, p.PublishedAt))
            .FirstOrDefaultAsync();
    }

    // Returns null when the author does not exist.
    public async Task<PostResponseDto?> CreateAsync(CreatePostDto dto)
    {
        // Validation has already checked that UserId is present.
        var userId = dto.UserId!.Value;

        if (!await _db.Users.AnyAsync(u => u.Id == userId))
        {
            return null;
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title,
            Content = dto.Content,
            PublishedAt = DateTimeOffset.UtcNow
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        return ToDto(post);
    }

    public async Task<PostResponseDto?> UpdateAsync(Guid id, UpdatePostDto dto)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post is null)
        {
            return null;
        }

        post.Title = dto.Title;
        post.Content = dto.Content;
        await _db.SaveChangesAsync();

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
        return true;
    }

    private static PostResponseDto ToDto(Post post) =>
        new(post.Id, post.UserId, post.Title, post.Content, post.PublishedAt);
}
