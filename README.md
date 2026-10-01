# Blog API: step 06, ASP.NET Core Identity

Lesson: ASP.NET Core Identity (follow-along on the Blog API)

This repository holds the Blog API used across the ASP.NET Core lessons. Each lesson has its own branch. The starter for a lesson is the previous step's branch, and the finished code is the lesson's own branch.

## What this step adds

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.x
- `User` inherits from `IdentityUser<Guid>` and keeps `Name` and `CreatedAt`
- `ApplicationDbContext` inherits from `IdentityDbContext<User, IdentityRole<Guid>, Guid>`, which adds the `AspNet*` tables
- `AddIdentityCore<User>()` with roles and the EF Core stores, and unique emails
- The `AddIdentity` migration. It renames the `Users` table to `AspNetUsers` and adds the Identity columns and tables
- `POST /auth/register` in `Endpoints/AuthEndpoints.cs`, using `UserManager<User>` through `Services/AuthService.cs`. Identity errors come back as a `400` ValidationProblem
- Users are now created only through registration. `POST /users`, `PUT /users/{id}` and `DELETE /users/{id}` are removed, and `GET /users` and `GET /users/{id}` stay
- The seeder creates its users through `UserManager`, with the password `Passw0rd!`

## Password rules

Identity's default password policy is kept:

- at least 6 characters
- at least one uppercase letter, one lowercase letter, one digit and one non-alphanumeric character

`RegisterDto` asks for at least 8 characters, which is stricter than the default length and agrees with the other rules.

## Project layout

```
BlogApi.slnx
BlogApi/
  Program.cs
  Endpoints/   AuthEndpoints.cs, UserEndpoints.cs, PostEndpoints.cs
  Filters/     RejectEmptyIdFilter.cs
  Errors/      GlobalExceptionHandler.cs
  Dtos/        Auth/, Users/, Posts/
  Models/      User.cs, Post.cs
  Services/    IAuthService.cs, AuthService.cs, IUserService.cs, UserService.cs, IPostService.cs, PostService.cs
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

If you have a `blog.db` from step 05, the `AddIdentity` migration keeps your old users, but they have no password and cannot log in. Delete `BlogApi/blog.db` (or run `dotnet ef database drop --project BlogApi`) and start the app again, so the seeder creates users with passwords.

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

curl http://localhost:5080/users
```

## Endpoints

| Method | Route | Result |
|---|---|---|
| POST | /auth/register | 201, 400 |
| GET | /users | 200 |
| GET | /users/{id} | 200, 404 |
| GET | /users/{id}/posts | 200, 404 |
| GET | /posts?search={term} | 200 |
| GET | /posts/{id} | 200, 404 |
| POST | /posts | 201, 400 |
| PUT | /posts/{id} | 200, 400, 404 |
| DELETE | /posts/{id} | 204, 404 |

Data is stored in the SQLite file `BlogApi/blog.db`, which is ignored by Git.

## Next step

`step-07-jwt`
