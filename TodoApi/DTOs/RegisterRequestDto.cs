namespace TodoApi.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request model for user registration.
/// </summary>
public class RegisterRequestDto
{
    /// <summary>
    /// The user's email address. Must be unique and in valid email format.
    /// </summary>
    /// <example>user@example.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email format is invalid")]
    [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's password. Must be 8-128 characters with uppercase, lowercase, and digit.
    /// </summary>
    /// <example>SecurePass123</example>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(128, ErrorMessage = "Password cannot exceed 128 characters")]
    public string Password { get; set; } = string.Empty;
}
