namespace BlogApi.Dtos.Posts;

// PUT replaces the whole resource, so every editable field is sent.
public record UpdatePostDto(string Title, string Content);
