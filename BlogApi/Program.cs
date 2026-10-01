using BlogApi.Endpoints;
using BlogApi.Errors;
using BlogApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Validates DataAnnotations on endpoint parameters (the DTOs) before the handler runs.
// An invalid request gets a 400 response with the errors in ProblemDetails format.
builder.Services.AddValidation();

// Error responses use the ProblemDetails format (RFC 9457).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Register each interface with the class that implements it.
// These services keep their data in memory, so they are singletons: one instance
// is shared by every request for the lifetime of the app. With a scoped or transient
// lifetime each request would get a new, empty store.
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IPostService, PostService>();

var app = builder.Build();

// Catches exceptions thrown later in the pipeline and passes them to GlobalExceptionHandler.
app.UseExceptionHandler();

// Adds a ProblemDetails body to error responses that have none, such as a plain 404.
app.UseStatusCodePages();

app.MapUserEndpoints();
app.MapPostEndpoints();

app.Run();
