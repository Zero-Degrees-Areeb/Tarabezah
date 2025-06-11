using System;

namespace Tarabezah.Application.Dtos.Reservations;

/// <summary>
/// DTO for table information in a reservation
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