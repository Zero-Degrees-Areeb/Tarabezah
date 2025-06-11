using System;
using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Commands.BlockingTable;

/// <summary>
/// Command to block a table for a specific time period
/// </summary>
public record BlockingTableCommand : IRequest<BlockTableResponse>
{
    /// <summary>
    /// The GUID of the floorplan element instance to block
    /// </summary>
    public Guid FloorplanElementInstanceGuid { get; set; }

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
    /// Optional notes about the block
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Response for the BlockTable command
/// </summary>
public class BlockTableResponse
{
    /// <summary>
    /// The GUID of the created block
    /// </summary>
    public Guid BlockTableGuid { get; set; }

    /// <summary>
    /// The GUID of the floorplan element instance that was blocked
    /// </summary>
    public Guid FloorplanElementInstanceGuid { get; set; }

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
    /// Notes about the block (if any)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Details of the floorplan element that was blocked
    /// </summary>
    public FloorplanElementResponseDto? FloorplanElement { get; set; }
}