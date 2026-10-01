using System.Net;
using System.Net.Http.Json;
using BlogApi.Dtos.Users;

namespace BlogApi.Tests.Integration;

public class UserEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetUsers_ReturnsUsersInTheDatabase()
    {
        var ada = await _factory.AddUserAsync("ada");
        var client = _factory.CreateClient();

        var users = await client.GetFromJsonAsync<List<UserResponseDto>>("/users");

        Assert.NotNull(users);
        var user = Assert.Single(users);
        Assert.Equal(ada.Id, user.Id);
    }

    [Fact]
    public async Task GetUser_WhenMissing_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
