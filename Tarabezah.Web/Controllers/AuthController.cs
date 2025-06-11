using Microsoft.AspNetCore.Mvc;
using Tarabezah.Application.Common;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Web.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    
    // Hardcoded credentials for demo purposes
    private const string ValidUsername = "admin";
    private const string ValidPassword = "password123";

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    /// <param name="request">The login request containing username and password</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/login
    ///     {
    ///        "username": "admin",
    ///        "password": "password123"
    ///     }
    ///
    /// </remarks>
    /// <returns>Authentication result with user info and token if successful</returns>
    /// <response code="200">Returns the user information and token</response>
    /// <response code="401">If the username or password is incorrect</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);
        
        if (request.Username == ValidUsername && request.Password == ValidPassword)
        {
            _logger.LogInformation("Login successful for user: {Username}", request.Username);
            
            var response = new LoginResponseDto
            {
                Username = request.Username,
                FullName = "System Administrator",
                Token = GenerateFakeToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };
            
            return Ok(ApiResponse<LoginResponseDto>.Success(response, "Login successful"));
        }
        
        _logger.LogWarning("Login failed for user: {Username}", request.Username);
        return Unauthorized(ApiResponse<LoginResponseDto>.Unauthorized("Invalid username or password"));
    }

    private string GenerateFakeToken()
    {
        // This is just a fake token for demo purposes
        // In a real application, you would use JWT or another token mechanism
        return $"demo-token-{Guid.NewGuid():N}";
    }
}

// DTOs for login
public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}