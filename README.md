# Blog API: step 02, validation and error handling

Lesson: Blog API validation and error handling exercise

This repository holds the Blog API used across the ASP.NET Core lessons. Each lesson has its own branch. The starter for a lesson is the previous step's branch, and the finished code is the lesson's own branch.

## What this step adds

- DataAnnotations on the request DTOs (`[Required]`, `[StringLength]`, `[EmailAddress]`)
- Built-in validation with `builder.Services.AddValidation()`. Invalid bodies get a `400` with a ProblemDetails body listing the errors
- `CreatePostDto.UserId` is a `Guid?` with `[Required]`, so a missing `userId` gives a `400`
- `AddProblemDetails()`, `UseExceptionHandler()` and `UseStatusCodePages()`, so every error response uses the ProblemDetails format
- `Errors/GlobalExceptionHandler.cs`, an `IExceptionHandler` that maps `ConflictException` to `409` and unexpected exceptions to `500`
- `Errors/ConflictException.cs`, thrown when an email is already taken
- `Filters/RejectEmptyIdFilter.cs`, an endpoint filter that returns `400` for an empty GUID in the route

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

Data is kept in memory and is lost when the app stops.

## Next step

`step-03-openapi`
