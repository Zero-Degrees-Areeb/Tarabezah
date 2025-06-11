using System;
using System.Collections.Generic;
using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetCombinedTables;

/// <summary>
/// Query to get all combined tables for a floorplan
/// </summary>
public record GetCombinedTablesQuery : IRequest<IEnumerable<CombinedTableDto>>
{
    /// <summary>
    /// The GUID of the floorplan to get combined tables for
    /// </summary>
    public Guid FloorplanGuid { get; init; }
} 