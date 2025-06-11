using System;
using MediatR;

namespace Tarabezah.Application.Queries.GetBlockTableByFloorplanInstanceGuid;

/// <summary>
/// Query to get block table information by floorplan instance GUID
/// </summary>
public record GetBlockTableByFloorplanInstanceGuidQuery : IRequest<BlockTableDetailsDto>
{
    /// <summary>
    /// The GUID of the floorplan element instance
    /// </summary>
    public Guid FloorplanElementInstanceGuid { get; init; }

    public GetBlockTableByFloorplanInstanceGuidQuery(Guid floorplanElementInstanceGuid)
    {
        FloorplanElementInstanceGuid = floorplanElementInstanceGuid;
    }
}

/// <summary>
/// DTO containing block table details
/// </summary>
public class BlockTableDetailsDto
{
    /// <summary>
    /// The GUID of the floorplan element instance
    /// </summary>
    public Guid FloorplanElementInstanceGuid { get; set; }

    /// <summary>
    /// The table ID of the floorplan element
    /// </summary>
    public string TableId { get; set; } = string.Empty;

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
}