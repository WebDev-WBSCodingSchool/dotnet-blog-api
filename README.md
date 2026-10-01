# Blog API: step 01, minimal API

Lesson: Blog API exercise (Dependency Injection and in-memory services)

This repository holds the Blog API used across the ASP.NET Core lessons. Each lesson has its own branch. The starter for a lesson is the previous step's branch, and the finished code is the lesson's own branch.

## What this step adds

- A solution (`BlogApi.slnx`) with one project in `BlogApi/`
- `Models/User.cs` and `Models/Post.cs`
- Request and response DTOs in `Dtos/Users` and `Dtos/Posts`
- `IUserService` and `IPostService` with in-memory implementations in `Services/`
- Services registered in `Program.cs` as singletons, because they keep their data in memory
- CRUD endpoints for users and posts in `Endpoints/`, grouped with `MapGroup`
- `GET /users/{id}/posts` to list the posts of one user
- `201 Created` with a `Location` header when a user or post is created
- `PUT` for updates

## Project layout

```
BlogApi.slnx
BlogApi/
  Program.cs
  Endpoints/   UserEndpoints.cs, PostEndpoints.cs
  Dtos/        Users/, Posts/
  Models/      User.cs, Post.cs
  Services/    IUserService.cs, UserService.cs, IPostService.cs, PostService.cs
```

## Run it

Requires the .NET 10 SDK.

```bash
dotnet run --project BlogApi
```

The API listens on `http://localhost:5080`.

```bash
curl -i -X POST http://localhost:5080/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Ada","email":"ada@example.com"}'

curl http://localhost:5080/users
```

## Endpoints

| Method | Route | Result |
|---|---|---|
| GET | /users | 200 |
| GET | /users/{id} | 200, 404 |
| GET | /users/{id}/posts | 200, 404 |
| POST | /users | 201 |
| PUT | /users/{id} | 200, 404 |
| DELETE | /users/{id} | 204, 404 |
| GET | /posts | 200 |
| GET | /posts/{id} | 200, 404 |
| POST | /posts | 201, 400 |
| PUT | /posts/{id} | 200, 404 |
| DELETE | /posts/{id} | 204, 404 |

Data is kept in memory and is lost when the app stops.

## Next step

`step-02-validation`
