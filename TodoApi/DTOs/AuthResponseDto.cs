namespace TodoApi.DTOs;

/// <summary>
/// Response model returned after successful authentication.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// The JWT token for authenticating subsequent requests.
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The UTC datetime when the token expires.
    /// </summary>
    /// <example>2026-02-02T18:30:00Z</example>
    public DateTime ExpiresAt { get; set; }
}
