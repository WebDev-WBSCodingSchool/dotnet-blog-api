using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BlogApi.Dtos.Auth;
using BlogApi.Dtos.Posts;
using BlogApi.Dtos.Users;

namespace BlogApi.Tests.Integration;

public class AuthEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task RegisterLoginAndCallProtectedEndpoints_WithRealJwt_Succeeds()
    {
        var client = _factory.CreateClient();

        // Register
        var register = await client.PostAsJsonAsync("/auth/register",
            new RegisterDto("Grace", "grace@example.com", "Str0ng!pass"));
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        // Log in and read the token
        var login = await client.PostAsJsonAsync("/auth/login",
            new LoginDto("grace@example.com", "Str0ng!pass"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var tokenResponse = await login.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(tokenResponse);

        // Send the token in the Authorization header
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        var me = await client.GetFromJsonAsync<UserResponseDto>("/auth/me");
        Assert.Equal("grace@example.com", me?.Email);

        var createPost = await client.PostAsJsonAsync("/posts", new CreatePostDto("Hello", "Written with a real token"));
        Assert.Equal(HttpStatusCode.Created, createPost.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithInvalidToken_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-token");

        var response = await client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/auth/register", new RegisterDto("Grace", "grace@example.com", "Str0ng!pass"));

        var response = await client.PostAsJsonAsync("/auth/login", new LoginDto("grace@example.com", "Wrong!pass1"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns400WithIdentityErrors()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/register",
            new RegisterDto("Grace", "grace@example.com", "password"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("PasswordRequiresDigit", problem.Errors.Keys);
    }
}
