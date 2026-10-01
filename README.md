# Blog API: step 05, querying and seeding

Lesson: Querying and Saving Data, and Seeding

This repository holds the Blog API used across the ASP.NET Core lessons. Each lesson has its own branch. The starter for a lesson is the previous step's branch, and the finished code is the lesson's own branch.

## What this step adds

- `PostResponseDto` has an `AuthorName`. The read queries get it by projecting `p.User.Name` inside `Select`, which makes EF Core join the `Users` table
- `PostService.UpdateAsync` loads the author with `Include`, so the tracked post and its user come back in one query
- `GET /posts?search=term` filters posts by title or content, building the query step by step
- `Data/DbSeeder.cs` adds two users and three posts when the database is empty
- In Development, `Program.cs` creates a scope, applies pending migrations with `MigrateAsync` and runs the seeder

No model changes, so there is no new migration in this step.

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
  Data/        ApplicationDbContext.cs, DbSeeder.cs, Migrations/
dotnet-tools.json
```

## Run it

Requires the .NET 10 SDK. Run these commands from the repository root.

```bash
# Install the dotnet-ef version pinned in dotnet-tools.json
dotnet tool restore

dotnet run --project BlogApi
```

In Development the app applies migrations and seeds the database on startup. To apply migrations without running the app:

```bash
dotnet ef database update --project BlogApi
```

To start again with a fresh database, stop the app and delete `BlogApi/blog.db`.

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
| GET | /posts?search={term} | 200 |
| GET | /posts/{id} | 200, 404 |
| POST | /posts | 201, 400 |
| PUT | /posts/{id} | 200, 400, 404 |
| DELETE | /posts/{id} | 204, 404 |

Data is stored in the SQLite file `BlogApi/blog.db`, which is ignored by Git.

## Next step

`step-06-identity`
