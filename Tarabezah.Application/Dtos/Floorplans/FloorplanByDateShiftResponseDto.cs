using System;
using System.Collections.Generic;

namespace Tarabezah.Application.Dtos.Floorplans;

/// <summary>
/// Response DTO containing floorplan information for a specific date and shift
/// </summary>
public class FloorplanByDateShiftResponseDto
{
    /// <summary>
    /// List of floorplans for the restaurant
    /// </summary>
    public List<FloorplanInfo> Floorplans { get; set; } = new();

    /// <summary>
    /// Information about the upcoming or active reservation
    /// </summary>
    public ReservationInfoDto? UpcomingOrActiveReservation { get; set; }
}

/// <summary>
/// Information about a specific floorplan
/// </summary>
public class FloorplanInfo
{
    /// <summary>
    /// The unique identifier of the floorplan
    /// </summary>
    public Guid FloorplanGuid { get; set; }

    /// <summary>
    /// The name of the floorplan
    /// </summary>
    public string FloorplanName { get; set; } = string.Empty;

    /// <summary>
    /// The list of elements in the floorplan
    /// </summary>
    public List<FloorplanElementDetailDto> Elements { get; set; } = new();
}

/// <summary>
/// Detailed information about a floorplan element
/// </summary>
public class FloorplanElementDetailDto
{
    /// <summary>
    /// The unique identifier of the element instance
    /// </summary>
    public Guid ElementInstanceGuid { get; set; }

    /// <summary>
    /// The unique identifier of the element type
    /// </summary>
    public Guid ElementTypeGuid { get; set; }

    /// <summary>
    /// The table ID within the floorplan
    /// </summary>
    public string TableId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the element
    /// </summary>
    public string ElementName { get; set; } = string.Empty;

    /// <summary>
    /// The URL of the element's image
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// The type of the element (e.g., Table, Chair)
    /// </summary>
    public string ElementType { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the element is currently blocked
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// The minimum capacity of the element
    /// </summary>
    public int MinCapacity { get; set; }

    /// <summary>
    /// The maximum capacity of the element
    /// </summary>
    public int MaxCapacity { get; set; }

    /// <summary>
    /// The X position of the element
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// The Y position of the element
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// The width of the element
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// The height of the element
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// The rotation of the element in degrees
    /// </summary>
    public double Rotation { get; set; }

    /// <summary>
    /// Whether the element is currently reserved
    /// </summary>
    public bool IsReserved { get; set; }

    /// <summary>
    /// Whether the element is currently reservable
    /// </summary>
    public bool IsReservable { get; set; }

    /// <summary>
    /// The reservation details for the element
    /// </summary>
    public List<ReservationInfoDto> ReservationInfo { get; set; } = new();

    /// <summary>
    /// The upcoming or active reservation for this element (starting in 15 minutes or currently active)
    /// </summary>
    public ReservationInfoDto? UpcomingOrActiveReservation { get; set; }

    /// <summary>
    /// The block table details if the table is blocked
    /// </summary>
    public BlockedTableDto? BlockedTable { get; set; }

    /// <summary>
    /// List of block table entries for the table on the requested date
    /// </summary>
    public List<BlockedTableDto> BlockedTables { get; set; } = new();

    /// <summary>
    /// Details about the combined table this element is part of
    /// </summary>
    public List<CombinedTableDetailsDto>? CombinedTableDetails { get; set; }
}

/// <summary>
/// Details about a combined table
/// </summary>
public class CombinedTableDetailsDto
{
    /// <summary>
    /// The GUID of the combined table
    /// </summary>
    public Guid CombinedTableGuid { get; set; }

    /// <summary>
    /// The name of the combined table
    /// </summary>
    public string CombinedTableName { get; set; } = string.Empty;

    /// <summary>
    /// Total minimum capacity of the combined table
    /// </summary>
    public int TotalMinCapacity { get; set; }

    /// <summary>
    /// Total maximum capacity of the combined table
    /// </summary>
    public int TotalMaxCapacity { get; set; }

    /// <summary>
    /// List of member tables in the combined table
    /// </summary>
    public List<CombinedTableMembersDto> MemberTables { get; set; } = new();

    /// <summary>
    /// Indicates whether the table can be reserved
    /// </summary>
    public bool HasCombination { get; set; }
}


/// <summary>
/// Details about a member table in a combined table
/// </summary>
public class CombinedTableMembersDto
{
    /// <summary>
    /// The table ID
    /// </summary>
    public string TableId { get; set; } = string.Empty;

    /// <summary>
    /// Minimum capacity of the table
    /// </summary>
    public int MinCapacity { get; set; }

    /// <summary>
    /// Maximum capacity of the table
    /// </summary>
    public int MaxCapacity { get; set; }
}

/// <summary>
/// Information about a reservation associated with a floorplan element
/// </summary>
public class ReservationInfoDto
{
    /// <summary>
    /// The unique identifier of the reservation
    /// </summary>
    public Guid ReservationGuid { get; set; }

    /// <summary>
    /// The name of the client
    /// </summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>
    /// The size of the party
    /// </summary>
    public int PartySize { get; set; }

    /// <summary>
    /// The reservation time
    /// </summary>
    public string ReservationTime { get; set; } = string.Empty;

    /// <summary>
    /// The end time of the reservation
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// The status of the reservation
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the reservation in format "Xh.Ym" (e.g., "1h.30m"). Only present for Seated reservations.
    /// </summary>
    public string? Duration { get; set; }
}

public class BlockedTableDto
{
    public Guid Guid { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Notes { get; set; }
}