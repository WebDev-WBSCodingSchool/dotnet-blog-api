# Blog API

A minimal Blog API built with ASP.NET Core using Minimal APIs. This project demonstrates clean architecture principles with DTOs, in-memory persistence, and extension methods for endpoint organization.

## Overview

This is a solution to the **Blog API** exercise that showcases:

- **Minimal APIs** with ASP.NET Core
- **Extension methods** for endpoint organization
- **DTOs** for request/response contracts
- **In-memory persistence** using services
- **One-to-many relationship** between Users and Posts

## Features

- **User Management**: Create, read, update, and delete users
- **Post Management**: Create, read, update, and delete blog posts
- **Relationships**: Each post belongs to exactly one user; users can have multiple posts
- **Data Validation**: User existence validation when creating posts
- **Cascade Protection**: Users with posts cannot be deleted

## Architecture

The project follows a clean, layered architecture:

```
BlogApi/
├── Program.cs                    # Application entry point and service configuration
├── Models/                       # Domain entities
│   ├── User.cs                  # User entity
│   └── Post.cs                  # Post entity
├── Dtos/                        # Data Transfer Objects
│   ├── Users/
│   │   ├── CreateUserDto.cs     # User creation request
│   │   ├── UpdateUserDto.cs     # User update request (partial)
│   │   └── UserResponseDto.cs   # User response
│   └── Posts/
│       ├── CreatePostDto.cs     # Post creation request
│       ├── UpdatePostDto.cs     # Post update request (partial)
│       └── PostResponseDto.cs   # Post response
├── Services/                    # Business logic layer
│   ├── Interfaces/
│   │   ├── IUserService.cs      # User service contract
│   │   └── IPostService.cs      # Post service contract
│   ├── InMemoryUserService.cs   # In-memory user operations
│   └── InMemoryPostService.cs   # In-memory post operations
└── Endpoints/                   # API endpoint definitions
    ├── UserEndpoints.cs         # User-related endpoints
    └── PostEndpoints.cs         # Post-related endpoints
```

## Data Model

### User

- `Id` (Guid) - Unique identifier
- `Name` (string) - User's display name
- `Email` (string) - User's email address
- `CreatedAt` (DateTimeOffset) - Account creation timestamp

### Post

- `Id` (Guid) - Unique identifier
- `UserId` (Guid) - Reference to the owning user
- `Title` (string) - Post title
- `Content` (string) - Post content
- `PublishedAt` (DateTimeOffset?) - Publication timestamp (nullable)

## API Endpoints

### Users

| Method   | Endpoint            | Description           | Request Body    | Response                                    |
| -------- | ------------------- | --------------------- | --------------- | ------------------------------------------- |
| `GET`    | `/users`            | Get all users         | -               | `UserResponseDto[]`                         |
| `GET`    | `/users/{id}`       | Get user by ID        | -               | `UserResponseDto` or `404`                  |
| `POST`   | `/users`            | Create new user       | `CreateUserDto` | `201` + `UserResponseDto` + Location header |
| `PATCH`  | `/users/{id}`       | Update user (partial) | `UpdateUserDto` | `UserResponseDto` or `404`                  |
| `DELETE` | `/users/{id}`       | Delete user           | -               | `204` or `404` or `400` (if user has posts) |
| `GET`    | `/users/{id}/posts` | Get user's posts      | -               | `PostResponseDto[]`                         |

### Posts

| Method   | Endpoint      | Description           | Request Body    | Response                                    |
| -------- | ------------- | --------------------- | --------------- | ------------------------------------------- |
| `GET`    | `/posts`      | Get all posts         | -               | `PostResponseDto[]`                         |
| `GET`    | `/posts/{id}` | Get post by ID        | -               | `PostResponseDto` or `404`                  |
| `POST`   | `/posts`      | Create new post       | `CreatePostDto` | `201` + `PostResponseDto` + Location header |
| `PATCH`  | `/posts/{id}` | Update post (partial) | `UpdatePostDto` | `PostResponseDto` or `404`                  |
| `DELETE` | `/posts/{id}` | Delete post           | -               | `204` or `404`                              |

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Your favorite code editor (VS Code recommended)

### Installation & Setup

1. **Clone the repository:**

   ```bash
   git clone https://github.com/WebDev-WBSCodingSchool/dotnet-blog-api.git
   cd dotnet-blog-api
   ```

2. **Restore dependencies:**

   ```bash
   dotnet restore
   ```

3. **Run the application:**

   ```bash
   dotnet run
   ```

4. **Access the API:**
   - Base URL: `https://localhost:7186` (or check console output for actual port)

### Alternative Setup (Creating from scratch)

## Usage Examples

### Create a User

```bash
curl -X POST https://localhost:7186/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john.doe@example.com"
  }'
```

### Create a Post

```bash
curl -X POST https://localhost:7186/posts \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "your-user-guid-here",
    "title": "My First Post",
    "content": "This is the content of my first blog post."
  }'
```

### Get All Users

```bash
curl https://localhost:7186/users
```

### Get User's Posts

```bash
curl https://localhost:7186/users/{userId}/posts
```

## Business Rules

1. **User-Post Relationship**: Each post must belong to an existing user
2. **Cascade Protection**: Users cannot be deleted if they have associated posts
3. **Partial Updates**: Both users and posts support partial updates via PATCH endpoints
4. **Validation**: Creating a post with a non-existent `UserId` returns `400 Bad Request`

## Design Decisions

### DTOs vs Entities

- **DTOs** are used at the HTTP boundary to control data exposure and validation
- **Entities** remain clean domain objects without serialization attributes
- Mapping between DTOs and entities happens in the endpoint handlers

### In-Memory Storage

- Uses `Dictionary<Guid, T>` for fast lookups by ID
- `List<T>` for operations requiring enumeration
- Services are registered as **singletons** to maintain shared state across requests

### Service Layer

- Interfaces define contracts for testability and future extensibility
- Async methods prepare for future database integration
- Business logic is encapsulated in services, not in endpoints

**Repository**: [https://github.com/WebDev-WBSCodingSchool/dotnet-blog-api](https://github.com/WebDev-WBSCodingSchool/dotnet-blog-api)
