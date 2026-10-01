using System.Net;
using System.Net.Http.Json;
using BlogApi.Dtos.Posts;

namespace BlogApi.Tests.Integration;

// xUnit creates a new instance of the class for every test, so every test
// gets a new factory and a new, empty database.
public class PostEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetPost_WhenMissing_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/posts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPost_WithEmptyGuid_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/posts/{Guid.Empty}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/posts", new CreatePostDto("Title", "Content"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_AsLoggedInUser_Returns201WithLocation()
    {
        var ada = await _factory.AddUserAsync("ada");
        var client = _factory.CreateClientAs(ada.Id);

        var response = await client.PostAsJsonAsync("/posts", new CreatePostDto("Title", "Content"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var post = await response.Content.ReadFromJsonAsync<PostResponseDto>();
        Assert.NotNull(post);
        Assert.Equal(ada.Id, post.UserId);
        Assert.Equal($"/posts/{post.Id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task CreatePost_WithEmptyTitle_Returns400()
    {
        var ada = await _factory.AddUserAsync("ada");
        var client = _factory.CreateClientAs(ada.Id);

        var response = await client.PostAsJsonAsync("/posts", new CreatePostDto("", "Content"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePost_ByAnotherUser_Returns403()
    {
        var ada = await _factory.AddUserAsync("ada");
        var alan = await _factory.AddUserAsync("alan");
        var adasPostId = await _factory.AddPostAsync(ada.Id, "Ada's post");
        var client = _factory.CreateClientAs(alan.Id);

        var response = await client.PutAsJsonAsync($"/posts/{adasPostId}", new UpdatePostDto("Hijacked", "Content"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePost_ByAuthor_Returns200()
    {
        var ada = await _factory.AddUserAsync("ada");
        var postId = await _factory.AddPostAsync(ada.Id, "Old title");
        var client = _factory.CreateClientAs(ada.Id);

        var response = await client.PutAsJsonAsync($"/posts/{postId}", new UpdatePostDto("New title", "New content"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var post = await response.Content.ReadFromJsonAsync<PostResponseDto>();
        Assert.Equal("New title", post?.Title);
    }

    [Fact]
    public async Task DeletePost_ByAuthor_Returns204AndPostIsGone()
    {
        var ada = await _factory.AddUserAsync("ada");
        var postId = await _factory.AddPostAsync(ada.Id, "To delete");
        var client = _factory.CreateClientAs(ada.Id);

        var deleteResponse = await client.DeleteAsync($"/posts/{postId}");
        var getResponse = await client.GetAsync($"/posts/{postId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
