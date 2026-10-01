using BlogApi.Dtos.Posts;
using BlogApi.Dtos.Users;
using BlogApi.Filters;
using BlogApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlogApi.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users")
            .AddEndpointFilter<RejectEmptyIdFilter>();

        group.MapGet("/", GetAllUsers);
        group.MapGet("/{id:guid}", GetUserById);
        group.MapGet("/{id:guid}/posts", GetPostsByUser);
        group.MapPost("/", CreateUser);
        group.MapPut("/{id:guid}", UpdateUser);
        group.MapDelete("/{id:guid}", DeleteUser);

        return group;
    }

    // The services are injected as handler parameters.
    private static async Task<Ok<IReadOnlyList<UserResponseDto>>> GetAllUsers(IUserService userService)
    {
        return TypedResults.Ok(await userService.GetAllAsync());
    }

    private static async Task<Results<Ok<UserResponseDto>, NotFound>> GetUserById(Guid id, IUserService userService)
    {
        var user = await userService.GetByIdAsync(id);
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
    }

    private static async Task<Results<Ok<IReadOnlyList<PostResponseDto>>, NotFound>> GetPostsByUser(
        Guid id, IUserService userService, IPostService postService)
    {
        if (!await userService.ExistsAsync(id))
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(await postService.GetByUserAsync(id));
    }

    private static async Task<Created<UserResponseDto>> CreateUser(CreateUserDto dto, IUserService userService)
    {
        var user = await userService.CreateAsync(dto);
        return TypedResults.Created($"/users/{user.Id}", user);
    }

    private static async Task<Results<Ok<UserResponseDto>, NotFound>> UpdateUser(
        Guid id, UpdateUserDto dto, IUserService userService)
    {
        var user = await userService.UpdateAsync(id, dto);
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteUser(
        Guid id, IUserService userService, IPostService postService)
    {
        if (!await userService.ExistsAsync(id))
        {
            return TypedResults.NotFound();
        }

        // Remove the user's posts first so no post points to a missing user.
        // Once the data lives in a database, cascade delete does this for us.
        await postService.DeleteByUserAsync(id);
        await userService.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}
