using System;
using MediatR;
using System.Collections.Generic;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Application.Queries.GetShiftTimeDuration;

/// <summary>
/// Query to get time duration information for a specific shift in a restaurant
/// </summary>
public record GetShiftTimeDurationQuery : IRequest<List<TimeSlotDto>>
{
    /// <summary>
    /// The GUID of the restaurant to get shift time duration for
    /// </summary>
    public Guid RestaurantGuid { get; init; }

    /// <summary>
    /// The GUID of the shift to get time duration for
    /// </summary>
    public Guid ShiftGuid { get; init; }

    /// <summary>
    /// The party size to check table availability for
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// The date to get shift time duration for
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// The table type to filter shifts by (optional)
    /// </summary>
    public string? TableType { get; init; }
}

/// <summary>
/// Response DTO containing time duration information for a shift
/// </summary>
public class ShiftTimeDurationDto
{
    /// <summary>
    /// The name of the shift
    /// </summary>
    public string ShiftName { get; set; } = string.Empty;

    /// <summary>
    /// The start time of the shift
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// The end time of the shift
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// List of table type groups with availability information
    /// </summary>
    public List<TableTypeGroupDto> TableTypeGroups { get; set; } = new List<TableTypeGroupDto>();
}

/// <summary>
/// DTO representing a table type group and its availability
/// </summary>
public class TableTypeGroupDto
{
    /// <summary>
    /// The table type
    /// </summary>
    public string TableType { get; set; } = string.Empty;

    /// <summary>
    /// List of time slots with availability information
    /// </summary>
    public List<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>();
}

/// <summary>
/// DTO representing a time slot and its availability
/// </summary>
public class TimeSlotDto
{
    /// <summary>
    /// The time of the slot
    /// </summary>
    public TimeSpan Time { get; set; }

    /// <summary>
    /// Total number of tables for this time slot
    /// </summary>
    public int TotalPatySizes { get; set; }

    /// <summary>
    /// Number of available tables for this time slot
    /// </summary>
    public int AllocatedTables { get; set; }

    /// <summary>
    /// Whether there are tables available at this time slot
    /// </summary>
    public bool IsAvailable { get; set; }
}