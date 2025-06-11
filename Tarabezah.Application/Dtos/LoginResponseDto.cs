namespace Tarabezah.Application.Dtos;

/// <summary>
/// Data transfer object for login responses
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Username of the authenticated user
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the authenticated user
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Authentication token to be used for subsequent requests
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Expiration date and time of the token in UTC
    /// </summary>
    public DateTime ExpiresAt { get; set; }
} 