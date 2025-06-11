using System;
using System.Collections.Generic;
using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Commands.CreateCombinedTable;

/// <summary>
/// Command to create a new combined table
/// </summary>
public record CreateCombinedTableCommand : IRequest<CombinedTableDto>
{
    /// <summary>
    /// The GUID of the floorplan to add the combined table to
    /// </summary>
    public Guid FloorplanGuid { get; init; }

    /// <summary>
    /// Optional name for the combined table group
    /// </summary>
    public string? GroupName { get; init; }

    /// <summary>
    /// The GUIDs of the floorplan element instances to include in the combined table
    /// </summary>
    public List<Guid> FloorplanElementInstanceGuids { get; init; } = new();

    /// <summary>
    /// Minimum capacity of the combined table
    /// </summary>
    public int? MinCapacity { get; init; }

    /// <summary>
    /// Maximum capacity of the combined table
    /// </summary>
    public int? MaxCapacity { get; init; }
}