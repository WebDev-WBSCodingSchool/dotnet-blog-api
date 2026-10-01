using BlogApi.Endpoints;
using BlogApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Register each interface with the class that implements it.
// These services keep their data in memory, so they are singletons: one instance
// is shared by every request for the lifetime of the app. With a scoped or transient
// lifetime each request would get a new, empty store.
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IPostService, PostService>();

var app = builder.Build();

app.MapUserEndpoints();
app.MapPostEndpoints();

app.Run();
