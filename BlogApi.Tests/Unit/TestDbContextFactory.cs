using BlogApi.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Tests.Unit;

// Creates an ApplicationDbContext backed by the EF Core in-memory provider.
// Each call uses a new database name, so every test starts with an empty database
// and no test can see data left behind by another.
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"BlogApiUnitTests-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
