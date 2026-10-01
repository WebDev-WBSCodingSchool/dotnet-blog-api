using System.Diagnostics.Metrics;

namespace BlogApi.Services;

// Custom metrics for the Blog API. Registered as a singleton, because a Meter
// and its instruments should live as long as the app.
public class BlogMetrics
{
    public const string MeterName = "BlogApi";

    private readonly Counter<long> _postsCreated;

    // IMeterFactory is provided by ASP.NET Core. Meters created through it are
    // disposed with the app and can be isolated in tests.
    public BlogMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _postsCreated = meter.CreateCounter<long>(
            "blogapi.posts.created",
            unit: "{post}",
            description: "Number of posts created");
    }

    public void PostCreated() => _postsCreated.Add(1);
}
