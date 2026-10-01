using BlogApi.Dtos.Posts;
using BlogApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlogApi.Endpoints;

public static class PostEndpoints
{
    public static RouteGroupBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/posts");

        group.MapGet("/", GetAllPosts);
        group.MapGet("/{id:guid}", GetPostById);
        group.MapPost("/", CreatePost);
        group.MapPut("/{id:guid}", UpdatePost);
        group.MapDelete("/{id:guid}", DeletePost);

        return group;
    }

    private static async Task<Ok<IReadOnlyList<PostResponseDto>>> GetAllPosts(IPostService postService)
    {
        return TypedResults.Ok(await postService.GetAllAsync());
    }

    private static async Task<Results<Ok<PostResponseDto>, NotFound>> GetPostById(Guid id, IPostService postService)
    {
        var post = await postService.GetByIdAsync(id);
        return post is null ? TypedResults.NotFound() : TypedResults.Ok(post);
    }

    private static async Task<Results<Created<PostResponseDto>, BadRequest<string>>> CreatePost(
        CreatePostDto dto, IPostService postService)
    {
        var post = await postService.CreateAsync(dto);
        if (post is null)
        {
            return TypedResults.BadRequest($"User '{dto.UserId}' does not exist.");
        }

        return TypedResults.Created($"/posts/{post.Id}", post);
    }

    private static async Task<Results<Ok<PostResponseDto>, NotFound>> UpdatePost(
        Guid id, UpdatePostDto dto, IPostService postService)
    {
        var post = await postService.UpdateAsync(id, dto);
        return post is null ? TypedResults.NotFound() : TypedResults.Ok(post);
    }

    private static async Task<Results<NoContent, NotFound>> DeletePost(Guid id, IPostService postService)
    {
        return await postService.DeleteAsync(id) ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
