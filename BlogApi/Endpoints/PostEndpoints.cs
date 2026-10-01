using System.Security.Claims;
using BlogApi.Dtos.Posts;
using BlogApi.Filters;
using BlogApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlogApi.Endpoints;

public static class PostEndpoints
{
    public static RouteGroupBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/posts")
            .WithTags("Posts")
            .AddEndpointFilter<RejectEmptyIdFilter>();

        // Reading posts is public.
        group.MapGet("/", GetAllPosts)
            .WithName("GetAllPosts")
            .WithSummary("List posts, newest first, optionally filtered by a search term");

        group.MapGet("/{id:guid}", GetPostById)
            .WithName("GetPostById")
            .WithSummary("Get one post by id")
            .ProducesValidationProblem();

        // Writing posts needs a valid token. Without one the response is 401.
        group.MapPost("/", CreatePost)
            .RequireAuthorization()
            .WithName("CreatePost")
            .WithSummary("Create a post as the logged-in user")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:guid}", UpdatePost)
            .RequireAuthorization()
            .WithName("UpdatePost")
            .WithSummary("Replace the title and content of one of your posts")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}", DeletePost)
            .RequireAuthorization()
            .WithName("DeletePost")
            .WithSummary("Delete one of your posts")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return group;
    }

    // search is read from the query string, for example /posts?search=engine
    private static async Task<Ok<IReadOnlyList<PostResponseDto>>> GetAllPosts(string? search, IPostService postService)
    {
        return TypedResults.Ok(await postService.GetAllAsync(search));
    }

    private static async Task<Results<Ok<PostResponseDto>, NotFound>> GetPostById(Guid id, IPostService postService)
    {
        var post = await postService.GetByIdAsync(id);
        return post is null ? TypedResults.NotFound() : TypedResults.Ok(post);
    }

    // ClaimsPrincipal is the authenticated user, built from the token.
    private static async Task<Results<Created<PostResponseDto>, UnauthorizedHttpResult>> CreatePost(
        CreatePostDto dto, ClaimsPrincipal user, IPostService postService)
    {
        var post = await postService.CreateAsync(user.GetUserId(), dto);
        if (post is null)
        {
            // The token is valid, but its user no longer exists.
            return TypedResults.Unauthorized();
        }

        return TypedResults.Created($"/posts/{post.Id}", post);
    }

    private static async Task<Results<Ok<PostResponseDto>, NotFound, ForbidHttpResult>> UpdatePost(
        Guid id, UpdatePostDto dto, ClaimsPrincipal user, IPostService postService)
    {
        var post = await postService.GetByIdAsync(id);
        if (post is null)
        {
            return TypedResults.NotFound();
        }

        // Logged in, but not the author: 403 Forbidden.
        if (post.UserId != user.GetUserId())
        {
            return TypedResults.Forbid();
        }

        var updated = await postService.UpdateAsync(id, dto);
        return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
    }

    private static async Task<Results<NoContent, NotFound, ForbidHttpResult>> DeletePost(
        Guid id, ClaimsPrincipal user, IPostService postService)
    {
        var post = await postService.GetByIdAsync(id);
        if (post is null)
        {
            return TypedResults.NotFound();
        }

        if (post.UserId != user.GetUserId())
        {
            return TypedResults.Forbid();
        }

        return await postService.DeleteAsync(id) ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
