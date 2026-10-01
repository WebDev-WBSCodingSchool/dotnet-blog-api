namespace BlogApi.Dtos.Posts;

public record PostResponseDto(
    Guid Id,
    Guid UserId,
    string AuthorName,
    string Title,
    string Content,
    DateTimeOffset PublishedAt);
