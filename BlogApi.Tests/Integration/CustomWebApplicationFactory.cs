using BlogApi.Data;
using BlogApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogApi.Tests.Integration;

// Starts the real API in memory, with two changes for testing:
// the SQLite database is replaced by an in-memory one, and a fake auth scheme is added.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Each factory gets its own database, so tests that use different factories never share data.
    private readonly string _databaseName = $"BlogApiIntegrationTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Any name other than Development skips the Development-only block in Program.cs
        // (Scalar, MigrateAsync and seeding), so tests start with an empty database.
        builder.UseEnvironment("Testing");

        // appsettings.Development.json is not loaded in the Testing environment,
        // so give the app a signing key for the real JWT tests.
        builder.UseSetting("Jwt:Key", "integration-test-signing-key-that-is-longer-than-32-bytes");

        // ConfigureTestServices runs after Program.cs has registered its services,
        // so anything registered here replaces or adds to the real registrations.
        builder.ConfigureTestServices(services =>
        {
            // Remove the SQLite configuration that AddDbContext added in Program.cs ...
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

            // ... and register the in-memory provider instead.
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            // Add the fake scheme next to the real JWT scheme.
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            // RequireAuthorization() uses the default policy. Let it accept a user from either scheme.
            services.Configure<AuthorizationOptions>(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(
                        JwtBearerDefaults.AuthenticationScheme, TestAuthHandler.SchemeName)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });
    }

    // A client that is logged in as the given user through the fake scheme.
    public HttpClient CreateClientAs(Guid userId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        return client;
    }

    // Adds a user straight to the test database and returns it.
    public async Task<User> AddUserAsync(string name)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserName = $"{name}@example.com",
            Email = $"{name}@example.com",
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    // Adds a post straight to the test database and returns its id.
    public async Task<Guid> AddPostAsync(Guid userId, string title)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Content = "Content",
            PublishedAt = DateTimeOffset.UtcNow
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return post.Id;
    }
}
