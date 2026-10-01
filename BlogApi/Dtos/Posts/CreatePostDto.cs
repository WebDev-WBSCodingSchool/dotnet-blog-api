using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.Posts;

// The author is the logged-in user, read from the token, so the body has no UserId.
public record CreatePostDto(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    [Required, StringLength(10_000, MinimumLength = 1)] string Content);
