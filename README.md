# Blog API: step 08a, unit tests

Lesson: Unit Testing in ASP.NET

This repository holds the Blog API used across the ASP.NET Core lessons. Each lesson has its own branch. The starter for a lesson is the previous step's branch, and the finished code is the lesson's own branch.

## What this step adds

- A test project, `BlogApi.Tests`, created with `dotnet new xunit` and added to `BlogApi.slnx`
- Packages: xUnit 2.9.3, Moq 4.21.0 and `Microsoft.EntityFrameworkCore.InMemory` 10.0.x
- `Unit/TestDbContextFactory.cs` creates an in-memory `ApplicationDbContext` with a new database name for every test
- `Unit/PostServiceTests.cs` tests `PostService` against that in-memory database
- `Unit/AuthServiceTests.cs` tests `AuthService` with a mocked `UserManager<User>` (Moq) and checks the `sub` claim of the issued token
- No changes to the API project

This branch is the starting point for the Integration Testing in ASP.NET lesson. The `step-08-testing` branch adds the integration tests on top of it.

## The signing key

`appsettings.Development.json` contains a development key, so the project runs straight after cloning. HMAC-SHA256 needs a key of at least 32 bytes; this one is 64 characters.

In a real project the key must not be committed. Remove it from `appsettings.Development.json` and store it with the Secret Manager:

```bash
dotnet user-secrets set "Jwt:Key" "<a random string of at least 32 characters>" --project BlogApi
```

In production, set it as an environment variable (`Jwt__Key`) or in a key vault.

## Project layout

```
BlogApi.slnx
BlogApi/
  Program.cs
  Endpoints/   AuthEndpoints.cs, UserEndpoints.cs, PostEndpoints.cs, ClaimsPrincipalExtensions.cs
  Filters/     RejectEmptyIdFilter.cs
  Errors/      GlobalExceptionHandler.cs
  Dtos/        Auth/, Users/, Posts/
  Models/      User.cs, Post.cs
  Services/    IAuthService.cs, AuthService.cs, IUserService.cs, UserService.cs, IPostService.cs, PostService.cs
  Data/        ApplicationDbContext.cs, DbSeeder.cs, Migrations/
BlogApi.Tests/
  Unit/        TestDbContextFactory.cs, PostServiceTests.cs, AuthServiceTests.cs
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

If you have a `blog.db` from step 05, the `AddIdentity` migration keeps your old users, but they have no password and cannot log in. Delete `BlogApi/blog.db` (or run `dotnet ef database drop --project BlogApi`) and start the app again, so the seeder creates users with passwords.

Run the tests:

```bash
dotnet test
```

To add a migration after changing the model:

```bash
dotnet ef migrations add <Name> --project BlogApi -o Data/Migrations
```

The API listens on `http://localhost:5080`.

- OpenAPI document: http://localhost:5080/openapi/v1.json
- Scalar API reference: http://localhost:5080/scalar

```bash
curl -i -X POST http://localhost:5080/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Grace","email":"grace@example.com","password":"Str0ng!pass"}'

curl -X POST http://localhost:5080/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"grace@example.com","password":"Str0ng!pass"}'

# Copy the token from the response
TOKEN=<token>

curl http://localhost:5080/auth/me -H "Authorization: Bearer $TOKEN"

curl -i -X POST http://localhost:5080/posts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"title":"Hello","content":"My first post"}'
```

The seeded users `ada@example.com` and `alan@example.com` can log in with `Passw0rd!`.

In Scalar, paste the token into the Authentication box to call the protected endpoints.

## Endpoints

| Method | Route | Result |
|---|---|---|
| POST | /auth/register | 201, 400 |
| POST | /auth/login | 200, 400, 401 |
| GET | /auth/me (token) | 200, 401, 404 |
| GET | /users | 200 |
| GET | /users/{id} | 200, 404 |
| GET | /users/{id}/posts | 200, 404 |
| GET | /posts?search={term} | 200 |
| GET | /posts/{id} | 200, 404 |
| POST | /posts (token) | 201, 400, 401 |
| PUT | /posts/{id} (token, author only) | 200, 400, 401, 403, 404 |
| DELETE | /posts/{id} (token, author only) | 204, 401, 403, 404 |

Data is stored in the SQLite file `BlogApi/blog.db`, which is ignored by Git.

## Next step

`step-08-testing`
