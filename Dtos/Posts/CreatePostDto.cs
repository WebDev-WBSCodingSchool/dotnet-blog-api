namespace BlogApi.Dtos.Posts;

public record CreatePostDto(Guid UserId, string Title, string Content);