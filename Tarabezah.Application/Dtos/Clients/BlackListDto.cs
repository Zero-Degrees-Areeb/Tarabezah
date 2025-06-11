using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Dtos.Clients;

/// <summary>
/// Data transfer object for blocked client information
/// </summary>
public class BlackListDto
{
    /// <summary>
    /// The unique identifier for the blocking record
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// The blocked client information
    /// </summary>
    public BlackListedDto Client { get; set; } = null!;

    /// <summary>
    /// The restaurant that blocked the client
    /// </summary>
    public RestaurantResponseDto Restaurant { get; set; } = null!;

    /// <summary>
    /// The reason for blocking the client (optional)
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// The date when the client was blocked
    /// </summary>
    public DateTime BlockedDate { get; set; }
}