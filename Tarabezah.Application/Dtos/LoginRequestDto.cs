using System.ComponentModel.DataAnnotations;

namespace Tarabezah.Application.Dtos;

/// <summary>
/// Data transfer object for login requests
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Username for authentication
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
} 