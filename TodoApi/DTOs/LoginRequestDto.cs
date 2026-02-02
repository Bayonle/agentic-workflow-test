namespace TodoApi.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request model for user authentication.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// The user's registered email address.
    /// </summary>
    /// <example>user@example.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email format is invalid")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's password.
    /// </summary>
    /// <example>SecurePass123</example>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
