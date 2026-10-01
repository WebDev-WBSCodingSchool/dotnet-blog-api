using BlogApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlogApi.Data;

// IdentityDbContext adds the Identity tables (AspNetUsers, AspNetRoles, ...) and a Users DbSet.
// The type arguments are our user class, the role class and the key type.
public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

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
        // Configures the Identity tables. Keep this call first.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(user =>
        {
            // Identity configures Email and UserName. We only add our own column.
            user.Property(u => u.Name).HasMaxLength(100).IsRequired();
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
