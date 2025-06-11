using System;
using System.Collections.Generic;

namespace Tarabezah.Application.Dtos.Reservations;

/// <summary>
/// Response DTO containing reservation counts and details grouped by status
/// </summary>
public class ReservationCountDto
{
    /// <summary>
    /// The date for which reservations are counted
    /// </summary>
    public DateTime ReservationDate { get; set; }

    /// <summary>
    /// The name of the shift
    /// </summary>
    public string ShiftName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of reservations across all statuses
    /// </summary>
    public int TotalReservations { get; set; }

    /// <summary>
    /// Total number of guests across all reservations
    /// </summary>
    public int TotalGuests { get; set; }

    /// <summary>
    /// Breakdown of reservations by status
    /// </summary>
    public List<ReservationStatusCountDto> ReservationCounts { get; set; } = new();
}

/// <summary>
/// Count of reservations for a specific status
/// </summary>
public class ReservationStatusCountDto
{
    /// <summary>
    /// The reservation status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Number of reservations with this status
    /// </summary>
    public int ReservationCount { get; set; }

    /// <summary>
    /// Total number of guests for reservations with this status
    /// </summary>
    public int GuestCount { get; set; }
} 