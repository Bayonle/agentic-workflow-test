namespace TodoApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs;
using TodoApi.Services;

/// <summary>
/// Handles user authentication and registration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration details (email and password)</param>
    /// <returns>201 Created on success, 400/409 on failure</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        // Model validation happens automatically via [ApiController]
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (succeeded, errorMessage) = await _authService.RegisterUserAsync(request.Email, request.Password);

        if (succeeded)
        {
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return CreatedAtAction(nameof(Register), new { message = "User registered successfully" });
        }

        // Check if duplicate email
        if (errorMessage?.Contains("already exists") == true)
        {
            _logger.LogWarning("Registration failed - duplicate email: {Email}", request.Email);
            return Conflict(new { message = errorMessage });
        }

        // Other validation errors (password complexity, etc.)
        _logger.LogWarning("Registration failed: {Error}", errorMessage);
        return BadRequest(new { message = errorMessage });
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials (email and password)</param>
    /// <returns>200 OK with token on success, 401 on failure</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        // Model validation happens automatically via [ApiController]
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (response, errorMessage) = await _authService.LoginUserAsync(request.Email, request.Password);

        if (response != null)
        {
            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(response);
        }

        _logger.LogWarning("Login failed for email: {Email}", request.Email);
        return Unauthorized(new { message = errorMessage });
    }
}
