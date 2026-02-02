namespace TodoApi.Services;

using TodoApi.DTOs;

public interface IAuthService
{
    /// <summary>
    /// Registers a new user with the provided email and password.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">User's password</param>
    /// <returns>
    /// Success: (true, null)
    /// Failure: (false, error message)
    /// </returns>
    Task<(bool Succeeded, string? ErrorMessage)> RegisterUserAsync(string email, string password);

    /// <summary>
    /// Authenticates a user and generates a JWT token.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">User's password</param>
    /// <returns>
    /// Success: (AuthResponseDto with token, null)
    /// Failure: (null, error message)
    /// </returns>
    Task<(AuthResponseDto? Response, string? ErrorMessage)> LoginUserAsync(string email, string password);
}
