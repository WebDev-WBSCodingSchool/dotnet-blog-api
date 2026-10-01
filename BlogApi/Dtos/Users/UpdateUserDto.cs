namespace BlogApi.Dtos.Users;

// PUT replaces the whole resource, so every editable field is sent.
public record UpdateUserDto(string Name, string Email);
