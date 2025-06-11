using System;
using System.Collections.Generic;

namespace Tarabezah.Application.Dtos;

/// <summary>
/// Data transfer object for combined tables
/// </summary>
public class CombinedTableDto
{
    /// <summary>
    /// The unique identifier for the combined table
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// The optional group name for this combined table
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// Minimum capacity of the combined table (sum of all member table minimum capacities)
    /// </summary>
    public int? MinCapacity { get; set; }

    /// <summary>
    /// Maximum capacity of the combined table (sum of all member table maximum capacities)
    /// </summary>
    public int? MaxCapacity { get; set; }

    /// <summary>
    /// Total capacity of all tables combined (average of min and max capacities)
    /// </summary>
    public int TotalCapacity { get; set; }

    /// <summary>
    /// The collection of table instances that are part of this combined table
    /// </summary>
    public List<CombinedTableMemberDto> Members { get; set; } = new List<CombinedTableMemberDto>();
}

/// <summary>
/// Data transfer object for members of a combined table
/// </summary>
public class CombinedTableMemberDto
{
    /// <summary>
    /// The unique identifier for the combined table member
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// The unique identifier for the referenced floorplan element instance
    /// </summary>
    public Guid FloorplanElementInstanceGuid { get; set; }

    /// <summary>
    /// The table ID of the referenced floorplan element instance
    /// </summary>
    public string TableId { get; set; } = string.Empty;

    /// <summary>
    /// The minimum capacity of the referenced floorplan element instance
    /// </summary>
    public int MinCapacity { get; set; }

    /// <summary>
    /// The maximum capacity of the referenced floorplan element instance
    /// </summary>
    public int MaxCapacity { get; set; }
}