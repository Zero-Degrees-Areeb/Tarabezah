using System;
using System.Collections.Generic;
using Tarabezah.Domain.Common;

namespace Tarabezah.Domain.Entities;

/// <summary>
/// Represents the status of a reservation
/// </summary>
public enum ReservationStatus
{
    Confirmed,
    Seated,
    Completed,
    Rejected,
    NoShow,
    Upcoming,
    Cancelled
}

/// <summary>
/// Represents the type of reservation
/// </summary>
public enum ReservationType
{
    OnCall,
    WalkIn
}

/// <summary>
/// Represents a reservation for a specific client, shift, and date
/// </summary>
public class Reservation : BaseEntity
{
    /// <summary>
    /// The ID of the client making the reservation
    /// </summary>
    public int? ClientId { get; set; }

    /// <summary>
    /// The client making the reservation
    /// </summary>
    public Client? Client { get; set; }

    /// <summary>
    /// The ID of the shift for the reservation
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// The shift for the reservation
    /// </summary>
    public Shift Shift { get; set; } = null!;

    /// <summary>
    /// The date of the reservation
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The time of the reservation within the shift
    /// </summary>
    public TimeSpan Time { get; set; }

    /// <summary>
    /// The number of people in the party
    /// </summary>
    public int PartySize { get; set; }

    /// <summary>
    /// The status of the reservation (null when first created)
    /// </summary>
    public ReservationStatus? Status { get; set; }

    /// <summary>
    /// The type of the reservation (OnCall or WalkIn)
    /// </summary>
    public ReservationType Type { get; set; } = ReservationType.OnCall;

    /// <summary>
    /// Tags specific to this reservation (e.g., window seat, high chair)
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Special notes or instructions for this reservation
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// Duration for this reservation
    /// </summary>
    public int? Duration { get; set; } = 300;

    /// <summary>
    /// The ID of the reserved table (FloorplanElementInstance)
    /// </summary>
    public int? ReservedElementId { get; set; }

    /// <summary>
    /// The reserved table for this reservation
    /// </summary>
    public FloorplanElementInstance? ReservedElement { get; set; }

    /// <summary>
    /// The ID of the combined table member this reservation is assigned to
    /// </summary>
    public int? CombinedTableMemberId { get; set; }

    /// <summary>
    /// The combined table member this reservation is assigned to
    /// </summary>
    public CombinedTableMember? CombinedTableMember { get; set; }
}