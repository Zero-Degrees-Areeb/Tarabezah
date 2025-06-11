using System;

namespace Tarabezah.Application.Dtos.Reservations;

/// <summary>
/// Detailed information about a reservation
/// </summary>
public class ReservationDetailDto
{
    /// <summary>
    /// The date and time of the reservation
    /// </summary>
    public DateTime ReservationTime { get; set; }

    public void ConvertToJordanTime(TimeZoneInfo jordanTimeZone)
    {
        ReservationTime = TimeZoneInfo.ConvertTimeFromUtc(ReservationTime, jordanTimeZone);
    }

    /// <summary>
    /// The name of the client
    /// </summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>
    /// The number of guests in the party
    /// </summary>
    public int GuestCount { get; set; }

    /// <summary>
    /// The size of the party
    /// </summary>
    public int PartySize { get; set; }

    /// <summary>
    /// Any special notes or instructions for the reservation
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Information about the assigned table, if any
    /// </summary>
    public TableInfoDto? TableInfo { get; set; }
}

/// <summary>
/// Information about a table assigned to a reservation
/// </summary>
public class TableInfoDto
{
    /// <summary>
    /// The unique identifier for the table within the floorplan
    /// </summary>
    public string TableId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the table
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// The GUID of the element
    /// </summary>
    public Guid ElementGuid { get; set; }

    /// <summary>
    /// The minimum seating capacity of the table
    /// </summary>
    public int MinCapacity { get; set; }

    /// <summary>
    /// The maximum seating capacity of the table
    /// </summary>
    public int MaxCapacity { get; set; }

    /// <summary>
    /// The name of the floorplan this table belongs to
    /// </summary>
    public string FloorplanName { get; set; } = string.Empty;
} 