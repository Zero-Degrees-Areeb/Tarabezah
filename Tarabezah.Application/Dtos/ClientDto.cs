using System;
using System.Collections.Generic;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Application.Dtos;

/// <summary>
/// Data transfer object for client information
/// </summary>
public class ClientDto
{
    /// <summary>
    /// The unique identifier for the client
    /// </summary>
    public Guid Guid { get; set; }

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
    /// String representation of the source
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// String representation of the tags as stored in the database
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Internal notes about the client
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Date when the client was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Date of the client's most recent visit (completed reservation) at the current restaurant
    /// </summary>
    public DateTime? LastVisitDate { get; set; }

    /// <summary>
    /// Number of completed reservations in the current restaurant
    /// </summary>
    public int CompletedReservationsCurrentRestaurant { get; set; }

    /// <summary>
    /// Number of completed reservations in other restaurants
    /// </summary>
    public int CompletedReservationsOtherRestaurants { get; set; }

    /// <summary>
    /// Number of no-show reservations in the current restaurant
    /// </summary>
    public int NoShowReservationsCurrentRestaurant { get; set; }

    /// <summary>
    /// Number of no-show reservations in other restaurants
    /// </summary>
    public int NoShowReservationsOtherRestaurants { get; set; }

    /// <summary>
    /// Number of cancelled reservations in the current restaurant
    /// </summary>
    public int CancelledReservationsCurrentRestaurant { get; set; }

    /// <summary>
    /// Number of cancelled reservations in other restaurants
    /// </summary>
    public int CancelledReservationsOtherRestaurants { get; set; }

    /// <summary>
    /// Information about blocks for this client
    /// </summary>
    public BlackListed BlackList { get; set; } = new BlackListed();
}

/// <summary>
/// Information about client blocks across restaurants
/// </summary>
public class BlackListed
{
    /// <summary>
    /// Whether the client is blocked in the current restaurant
    /// </summary>
    public bool Same { get; set; }

    /// <summary>
    /// Number of blocks in other restaurants
    /// </summary>
    public int Others { get; set; }
}

public class BlackListedDto
{
    /// <summary>
    /// The unique identifier for the client
    /// </summary>
    public Guid Guid { get; set; }

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
    /// String representation of the source
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// String representation of the tags as stored in the database
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Internal notes about the client
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}