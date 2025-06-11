using System;
using Tarabezah.Domain.Common;

namespace Tarabezah.Domain.Entities;

/// <summary>
/// Represents a blocking relationship between a client and a restaurant
/// </summary>
public class BlackList : BaseEntity
{
    /// <summary>
    /// The ID of the client
    /// </summary>
    public int ClientId { get; set; }

    /// <summary>
    /// The client being blocked
    /// </summary>
    public Client Client { get; set; } = null!;

    /// <summary>
    /// The ID of the restaurant
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// The restaurant blocking the client
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;

    /// <summary>
    /// The reason for blocking the client (optional)
    /// </summary>
    public string? Reason { get; set; } = null;

    /// <summary>
    /// The date when the client was blocked
    /// </summary>
    public DateTime BlockedDate { get; set; } = DateTime.Now;
}