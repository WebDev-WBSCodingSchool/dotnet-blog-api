using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace BlogApi.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        // Runs every registered check.
        app.MapHealthChecks("/health");

        // Readiness: can the app serve requests? Runs only the checks tagged "ready" (the database).
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        // Liveness: is the process running? Runs no checks, so it answers
        // Healthy as long as the app can handle a request.
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        return app;
    }
}
