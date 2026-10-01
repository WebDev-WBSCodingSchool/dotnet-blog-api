using System.Diagnostics.Metrics;
using BlogApi.Data;
using BlogApi.Dtos.Posts;
using BlogApi.Models;
using BlogApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlogApi.Tests.Unit;

public class PostServiceTests
{
    private readonly ApplicationDbContext _db;
    private readonly PostService _service;

    // xUnit creates a new instance of this class for every test,
    // so each test gets its own database and service.
    public PostServiceTests()
    {
        _db = TestDbContextFactory.Create();

        // NullLogger discards log messages. The tests do not check what is logged.
        var logger = NullLogger<PostService>.Instance;

        // BlogMetrics needs an IMeterFactory. AddMetrics registers the default one.
        var meterFactory = new ServiceCollection()
            .AddMetrics()
            .BuildServiceProvider()
            .GetRequiredService<IMeterFactory>();

        _service = new PostService(_db, logger, new BlogMetrics(meterFactory));
    }

    private async Task<User> AddUserAsync(string name)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserName = $"{name}@example.com",
            Email = $"{name}@example.com",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    private async Task<Post> AddPostAsync(User author, string title, string content, DateTimeOffset publishedAt)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = author.Id,
            Title = title,
            Content = content,
            PublishedAt = publishedAt
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();
        return post;
    }

    [Fact]
    public async Task CreateAsync_WithExistingUser_SavesPostAndReturnsAuthorName()
    {
        // Arrange
        var ada = await AddUserAsync("ada");
        var dto = new CreatePostDto("Hello", "First post");

        // Act
        var result = await _service.CreateAsync(ada.Id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello", result.Title);
        Assert.Equal(ada.Id, result.UserId);
        Assert.Equal("ada", result.AuthorName);
        Assert.Single(_db.Posts);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownUser_ReturnsNullAndSavesNothing()
    {
        var result = await _service.CreateAsync(Guid.NewGuid(), new CreatePostDto("Hello", "Body"));

        Assert.Null(result);
        Assert.Empty(_db.Posts);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsNewestPostFirst()
    {
        var ada = await AddUserAsync("ada");
        var now = DateTimeOffset.UtcNow;
        await AddPostAsync(ada, "Older", "Body", now.AddDays(-2));
        await AddPostAsync(ada, "Newer", "Body", now.AddDays(-1));

        var posts = await _service.GetAllAsync(search: null);

        Assert.Equal(["Newer", "Older"], posts.Select(p => p.Title));
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_ReturnsOnlyMatchingPosts()
    {
        var ada = await AddUserAsync("ada");
        var now = DateTimeOffset.UtcNow;
        await AddPostAsync(ada, "About engines", "Body", now);
        await AddPostAsync(ada, "Something else", "Mentions an engine too", now);
        await AddPostAsync(ada, "Unrelated", "Nothing here", now);

        var posts = await _service.GetAllAsync(search: "engine");

        Assert.Equal(2, posts.Count);
        Assert.DoesNotContain(posts, p => p.Title == "Unrelated");
    }

    [Fact]
    public async Task UpdateAsync_WithExistingPost_ReplacesTitleAndContent()
    {
        var ada = await AddUserAsync("ada");
        var post = await AddPostAsync(ada, "Old title", "Old content", DateTimeOffset.UtcNow);

        var result = await _service.UpdateAsync(post.Id, new UpdatePostDto("New title", "New content"));

        Assert.NotNull(result);
        Assert.Equal("New title", result.Title);
        Assert.Equal("New content", result.Content);
        Assert.Equal("ada", result.AuthorName);
    }

    [Fact]
    public async Task UpdateAsync_WithMissingPost_ReturnsNull()
    {
        var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdatePostDto("Title", "Content"));

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingPost_RemovesItAndReturnsTrue()
    {
        var ada = await AddUserAsync("ada");
        var post = await AddPostAsync(ada, "Title", "Content", DateTimeOffset.UtcNow);

        var deleted = await _service.DeleteAsync(post.Id);

        Assert.True(deleted);
        Assert.Empty(_db.Posts);
    }

    [Fact]
    public async Task DeleteAsync_WithMissingPost_ReturnsFalse()
    {
        var deleted = await _service.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
    }
}
