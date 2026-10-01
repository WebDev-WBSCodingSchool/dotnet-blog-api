using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos.Auth;

// Identity's default password rules: at least 6 characters, with an uppercase letter,
// a lowercase letter, a digit and a non-alphanumeric character.
// MinLength(8) is stricter than the default length and does not contradict the other rules,
// which Identity checks when the user is created.
public record RegisterDto(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, MinLength(8)] string Password);
