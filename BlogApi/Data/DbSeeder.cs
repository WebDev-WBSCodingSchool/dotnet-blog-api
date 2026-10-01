using BlogApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data;

// Adds sample data so the API has something to show during development.
public static class DbSeeder
{
    // Every seeded user gets this password, so you can log in as them while developing.
    public const string DevPassword = "Passw0rd!";

    public static async Task SeedAsync(ApplicationDbContext db, UserManager<User> userManager)
    {
        // Only seed an empty database. Running the app again does not add duplicates.
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var ada = await CreateUserAsync(userManager, "Ada Lovelace", "ada@example.com", now.AddDays(-10));
        var alan = await CreateUserAsync(userManager, "Alan Turing", "alan@example.com", now.AddDays(-5));

        db.Posts.AddRange(
            new Post
            {
                Id = Guid.NewGuid(),
                UserId = ada.Id,
                Title = "Notes on the Analytical Engine",
                Content = "The engine weaves algebraic patterns just as the loom weaves flowers and leaves.",
                PublishedAt = now.AddDays(-9)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                UserId = ada.Id,
                Title = "Why programs need testing",
                Content = "A machine does exactly what it is told, so the instructions must be right.",
                PublishedAt = now.AddDays(-3)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                UserId = alan.Id,
                Title = "Can machines think?",
                Content = "A better question is whether a machine can do well in the imitation game.",
                PublishedAt = now.AddDays(-1)
            });

        await db.SaveChangesAsync();
    }

    // Users go through UserManager so their passwords are hashed like any registered user.
    private static async Task<User> CreateUserAsync(
        UserManager<User> userManager, string name, string email, DateTimeOffset createdAt)
    {
        var user = new User
        {
            Name = name,
            UserName = email,
            Email = email,
            CreatedAt = createdAt
        };

        var result = await userManager.CreateAsync(user, DevPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not seed user {email}: {errors}");
        }

        return user;
    }
}
