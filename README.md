# Blog API: step 04, EF Core and SQLite

Lesson: EF Core Fundamentals tutorial

This repository holds the Blog API used across the ASP.NET Core lessons. Each lesson has its own branch. The starter for a lesson is the previous step's branch, and the finished code is the lesson's own branch.

## What this step adds

- `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Design` 10.0.x
- A local tool manifest (`dotnet-tools.json`) that pins `dotnet-ef` 10.0.x
- `Data/ApplicationDbContext.cs` with a one-to-many relationship: a user has many posts, and deleting a user deletes their posts (cascade delete)
- Navigation properties `User.Posts` and `Post.User`
- A unique index on `User.Email`
- A value converter that stores `DateTimeOffset` as a number, so SQLite can sort by it
- The connection string `DefaultConnection` in `appsettings.json` (`Data Source=blog.db`)
- The `InitialCreate` migration in `Data/Migrations`
- `UserService` and `PostService` rewritten to use the DbContext, registered as scoped
- Read queries use `AsNoTracking()` and project straight to DTOs with `Select`

## Project layout

```
BlogApi.slnx
BlogApi/
  Program.cs
  Endpoints/   UserEndpoints.cs, PostEndpoints.cs
  Filters/     RejectEmptyIdFilter.cs
  Errors/      ConflictException.cs, GlobalExceptionHandler.cs
  Dtos/        Users/, Posts/
  Models/      User.cs, Post.cs
  Services/    IUserService.cs, UserService.cs, IPostService.cs, PostService.cs
  Data/        ApplicationDbContext.cs, Migrations/
dotnet-tools.json
```

## Run it

Requires the .NET 10 SDK. Run these commands from the repository root.

```bash
# Install the dotnet-ef version pinned in dotnet-tools.json
dotnet tool restore

# Restore packages and build once, so dotnet ef can read the project
dotnet build

# Create BlogApi/blog.db and apply the migrations
dotnet ef database update --project BlogApi

dotnet run --project BlogApi
```

To add a migration after changing the model:

```bash
dotnet ef migrations add <Name> --project BlogApi -o Data/Migrations
```

The API listens on `http://localhost:5080`.

- OpenAPI document: http://localhost:5080/openapi/v1.json
- Scalar API reference: http://localhost:5080/scalar

```bash
curl -i -X POST http://localhost:5080/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Ada","email":"ada@example.com"}'

curl http://localhost:5080/users

# Invalid body: 400 with validation errors
curl -i -X POST http://localhost:5080/users \
  -H "Content-Type: application/json" \
  -d '{"name":"","email":"not-an-email"}'
```

## Endpoints

| Method | Route | Result |
|---|---|---|
| GET | /users | 200 |
| GET | /users/{id} | 200, 404 |
| GET | /users/{id}/posts | 200, 404 |
| POST | /users | 201, 400, 409 |
| PUT | /users/{id} | 200, 400, 404, 409 |
| DELETE | /users/{id} | 204, 404 |
| GET | /posts | 200 |
| GET | /posts/{id} | 200, 404 |
| POST | /posts | 201, 400 |
| PUT | /posts/{id} | 200, 400, 404 |
| DELETE | /posts/{id} | 204, 404 |

Data is stored in the SQLite file `BlogApi/blog.db`, which is ignored by Git.

## Next step

`step-05-queries-seeding`
