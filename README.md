# Blog API: step 03, OpenAPI

Lesson: Documenting APIs (OpenAPI exercise)

This repository holds the Blog API used across the ASP.NET Core lessons. Each lesson has its own branch. The starter for a lesson is the previous step's branch, and the finished code is the lesson's own branch.

## What this step adds

- `Microsoft.AspNetCore.OpenApi` 10.0.x: `AddOpenApi()` and `MapOpenApi()` generate an OpenAPI 3.1 document
- `Scalar.AspNetCore` 2.x: `MapScalarApiReference()` serves an interactive API reference
- Both are mapped only in the Development environment
- `WithTags`, `WithName` and `WithSummary` on every endpoint
- `ProducesValidationProblem()` and `ProducesProblem(409)` for responses that come from validation, filters or the exception handler. Responses returned through `TypedResults` are described automatically

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

Data is kept in memory and is lost when the app stops.

## Next step

`step-04-ef-core`
