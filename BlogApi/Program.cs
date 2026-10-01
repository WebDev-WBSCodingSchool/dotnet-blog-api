using System.Text;
using BlogApi.Data;
using BlogApi.Endpoints;
using BlogApi.Errors;
using BlogApi.Models;
using BlogApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog replaces the default logging providers. Every ILogger<T> in the app now writes through it.
// Minimum levels come from the "Serilog" section in appsettings.json.
builder.Services.AddSerilog((services, logger) => logger
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    // One file per day in BlogApi/logs. shared: true lets several app instances
    // (for example the integration tests) write to the same file.
    .WriteTo.File("logs/blog-api-.log", rollingInterval: RollingInterval.Day, shared: true));

// Validates DataAnnotations on endpoint parameters (the DTOs) before the handler runs.
// An invalid request gets a 400 response with the errors in ProblemDetails format.
builder.Services.AddValidation();

// Error responses use the ProblemDetails format (RFC 9457).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// The DbContext is registered as scoped: each request gets its own instance.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// AddIdentityCore registers UserManager<User>, the password hasher and the validators.
// It does not add cookie sign-in or UI, which a JSON API does not need.
builder.Services
    .AddIdentityCore<User>(options =>
    {
        // Registration uses the email as the user name, so emails must be unique.
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Authentication: read the "Authorization: Bearer <token>" header and check the token.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep the claim names from the token as they are ("sub", "email", "name").
        // Without this, "sub" would be renamed to ClaimTypes.NameIdentifier.
        options.MapInboundClaims = false;

        var key = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // The token must be signed with our key.
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

            // The token must come from our API and be meant for our API.
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // The token must not be expired. By default 5 minutes of clock difference are allowed.
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization: decides whether the authenticated user may call an endpoint.
builder.Services.AddAuthorization();

// The services use the DbContext, so they are scoped too. A singleton cannot depend on a
// scoped service, because it would keep one DbContext alive for the whole app.
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Custom metrics. A singleton, because the counters must live as long as the app.
builder.Services.AddSingleton<BlogMetrics>();

// Health checks. The DbContext check tries to connect to the database.
// The "ready" tag puts it in the /health/ready endpoint.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(tags: ["ready"]);

// Generates an OpenAPI document that describes every endpoint.
builder.Services.AddOpenApi(options =>
{
    // Describe the Bearer scheme, so Scalar shows an "Authentication" box for the token.
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste the token returned by POST /auth/login."
        };
        return Task.CompletedTask;
    });

    // Mark the endpoints that call RequireAuthorization() as needing the Bearer token.
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var requiresAuth = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAuthorizeData>()
            .Any();

        if (requiresAuth)
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });
        }
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Writes one log line per request with the method, path, status code and duration.
// It comes first so it also sees requests that end in an exception.
app.UseSerilogRequestLogging();

// Only expose the API documentation while developing.
if (app.Environment.IsDevelopment())
{
    // The OpenAPI document: /openapi/v1.json
    app.MapOpenApi();

    // Interactive API reference built from that document: /scalar
    app.MapScalarApiReference();

    // The DbContext is scoped, and no request is running yet, so create a scope by hand.
    // The scope and the DbContext are disposed at the end of this block.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    // Apply any pending migrations, then add sample data if the database is empty.
    // This only runs in Development. In production, migrations are applied as a separate step.
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, userManager);
}

// Catches exceptions thrown later in the pipeline and passes them to GlobalExceptionHandler.
app.UseExceptionHandler();

// Adds a ProblemDetails body to error responses that have none, such as a plain 404.
app.UseStatusCodePages();

// Authentication first (who are you?), then authorization (are you allowed?).
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapPostEndpoints();
app.MapHealthEndpoints();

app.Run();
