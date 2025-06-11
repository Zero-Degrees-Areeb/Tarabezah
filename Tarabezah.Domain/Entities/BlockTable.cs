using System;
using Tarabezah.Domain.Common;

namespace Tarabezah.Domain.Entities;

/// <summary>
/// Represents a table that is blocked for a specific time period
/// </summary>
public class BlockTable : BaseEntity
{
    /// <summary>
    /// The ID of the floorplan element instance that is blocked
    /// </summary>
    public int FloorplanElementInstanceId { get; set; }

    /// <summary>
    /// The floorplan element instance that is blocked
    /// </summary>
    public FloorplanElementInstance FloorplanElementInstance { get; set; } = null!;

    /// <summary>
    /// The start time of the block period
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// The end time of the block period
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// The start date of the block period
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The end date of the block period
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Optional notes about why the table is blocked
    /// </summary>
    public string? Notes { get; set; }
}