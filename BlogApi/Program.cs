using BlogApi.Data;
using BlogApi.Endpoints;
using BlogApi.Errors;
using BlogApi.Models;
using BlogApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

// The services use the DbContext, so they are scoped too. A singleton cannot depend on a
// scoped service, because it would keep one DbContext alive for the whole app.
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Generates an OpenAPI document that describes every endpoint.
builder.Services.AddOpenApi();

var app = builder.Build();

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

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapPostEndpoints();

app.Run();
