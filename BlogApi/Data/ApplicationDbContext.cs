using BlogApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlogApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite has no DateTimeOffset type, and EF Core cannot sort or compare
        // DateTimeOffset columns stored as text. Storing them as numbers fixes that.
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToBinaryConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(user =>
        {
            user.Property(u => u.Name).HasMaxLength(100).IsRequired();
            user.Property(u => u.Email).HasMaxLength(256).IsRequired();

            // No two users can share an email.
            user.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Post>(post =>
        {
            post.Property(p => p.Title).HasMaxLength(200).IsRequired();
            post.Property(p => p.Content).HasMaxLength(10_000).IsRequired();

            // One user has many posts. Deleting a user deletes their posts.
            post.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
