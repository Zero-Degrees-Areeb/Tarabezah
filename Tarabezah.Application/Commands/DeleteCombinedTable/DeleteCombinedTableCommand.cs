using System;
using MediatR;

namespace Tarabezah.Application.Commands.DeleteCombinedTable;

/// <summary>
/// Command to delete a combined table
/// </summary>
public record DeleteCombinedTableCommand : IRequest<bool>
{
    /// <summary>
    /// The GUID of the combined table to delete
    /// </summary>
    public Guid CombinedTableGuid { get; init; }

    /// <summary>
    /// The GUID of the floorplan this combined table belongs to (set by handler)
    /// </summary>
    public Guid? FloorplanGuid { get; set; }
}