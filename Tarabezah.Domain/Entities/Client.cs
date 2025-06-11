using System;
using System.Collections.Generic;
using Tarabezah.Domain.Common;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Domain.Entities;

/// <summary>
/// Represents a restaurant client
/// </summary>
public class Client : BaseEntity
{
    /// <summary>
    /// The name of the client
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The client's phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// The client's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The client's birthday (optional)
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// Where the client came from
    /// </summary>
    public ClientSource Source { get; set; } = ClientSource.Other;

    /// <summary>
    /// Tags describing the client's preferences
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Internal notes about the client
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Collection of client's reservations
    /// </summary>
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    /// <summary>
    /// Collection of restaurants that have blocked this client
    /// </summary>
    public ICollection<BlackList> BlockedByRestaurants { get; set; } = new List<BlackList>();
}