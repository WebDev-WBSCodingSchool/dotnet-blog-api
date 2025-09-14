using BlogApi.Dtos.Posts;
using BlogApi.Services;

namespace BlogApi.Endpoints;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/posts").WithTags("Posts");

        // GET /posts
        group.MapGet("/", async (IPostService postService) =>
        {
            var posts = await postService.ListAsync();
            var postDtos = posts.Select(p => new PostResponseDto(p.Id, p.UserId, p.Title, p.Content, p.PublishedAt));
            return Results.Ok(postDtos);
        });

        // GET /posts/{id:guid}
        group.MapGet("/{id:guid}", async (Guid id, IPostService postService) =>
        {
            var post = await postService.GetAsync(id);
            if (post == null)
                return Results.NotFound();

            var postDto = new PostResponseDto(post.Id, post.UserId, post.Title, post.Content, post.PublishedAt);
            return Results.Ok(postDto);
        });

        // POST /posts
        group.MapPost("/", async (CreatePostDto createPostDto, IPostService postService, HttpContext context) =>
        {
            try
            {
                var post = await postService.CreateAsync(createPostDto.UserId, createPostDto.Title, createPostDto.Content);
                var postDto = new PostResponseDto(post.Id, post.UserId, post.Title, post.Content, post.PublishedAt);

                var location = $"{context.Request.Scheme}://{context.Request.Host}/posts/{post.Id}";
                return Results.Created(location, postDto);
            }
            catch (ArgumentException)
            {
                return Results.BadRequest("User not found");
            }
        });

        // PATCH /posts/{id:guid}
        group.MapPatch("/{id:guid}", async (Guid id, UpdatePostDto updatePostDto, IPostService postService) =>
        {
            var post = await postService.UpdateAsync(id, updatePostDto.Title, updatePostDto.Content);
            if (post == null)
                return Results.NotFound();

            var postDto = new PostResponseDto(post.Id, post.UserId, post.Title, post.Content, post.PublishedAt);
            return Results.Ok(postDto);
        });

        // DELETE /posts/{id:guid}
        group.MapDelete("/{id:guid}", async (Guid id, IPostService postService) =>
        {
            var deleted = await postService.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}