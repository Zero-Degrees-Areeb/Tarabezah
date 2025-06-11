using System.Collections.Generic;
using Tarabezah.Application.Dtos.Reservations;
using System.Text.Json.Serialization;

namespace Tarabezah.Application.Queries.GetReservationsByDateAndShift;

/// <summary>
/// Response DTO containing grouped reservations and summary counts
/// </summary>
public class ReservationGroupsResponseDto
{
    public List<StatusGroupDto> StatusGroup { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

/// <summary>
/// DTO representing a group of reservations with the same status
/// </summary>
public class StatusGroupDto
{
    /// <summary>
    /// Reservation status
    /// </summary>
    public string ReservationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Number of reservations in this group
    /// </summary>
    public int ReservationCount { get; set; }

    /// <summary>
    /// Total number of guests in this group
    /// </summary>
    public int ReservationPartyCount { get; set; }

    /// <summary>
    /// List of reservations in this group
    /// </summary>
    public List<ReservationDetailDto> Reservations { get; set; } = new();
}

/// <summary>
/// DTO representing a reservation within a group
/// </summary>
public class ReservationDetailDto
{
    /// <summary>
    /// Id of the Reservation
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Id of the Reservation
    /// </summary>
    public string ClientGuid { get; set; } = string.Empty;

    /// <summary>
    /// Name of the client
    /// </summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>
    /// List of client tags
    /// </summary>
    public List<string> ClientTags { get; set; } = new();

    /// <summary>
    /// Information about the assigned tables
    /// </summary>
    public List<TableInfoDto> TableInfo { get; set; } = new();

    /// <summary>
    /// Time of the reservation
    /// </summary>
    public string Time { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the reservation is more than 15 minutes late
    /// </summary>
    public bool IsLate { get; set; }

    /// <summary>
    /// Status of the reservation
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Number of guests in the party
    /// </summary>
    public int PartySize { get; set; }

    /// <summary>
    /// Type of guest (e.g., OnCall, WalkIn)
    /// </summary>
    public string ReservationType { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes for the reservation
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// List of reservation tags
    /// </summary>
    public List<string> ReservationTags { get; set; } = new();
}

/// <summary>
/// DTO representing an assigned table
/// </summary>
public class AssignedTableDto
{
    /// <summary>
    /// ID of the floorplan element
    /// </summary>
    public Guid ElementId { get; set; }

    /// <summary>
    /// Name of the floorplan element
    /// </summary>
    public string ElementName { get; set; } = string.Empty;
}