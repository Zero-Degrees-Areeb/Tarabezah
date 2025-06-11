using System;
using System.Collections.Generic;
using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetRestaurantCombinedTables;

/// <summary>
/// Query to get all combined tables for all floorplans of a restaurant
/// </summary>
public record GetRestaurantCombinedTablesQuery : IRequest<List<FloorplanCombinedTablesDto>>
{
    /// <summary>
    /// The GUID of the restaurant to get combined tables for
    /// </summary>
    public Guid RestaurantGuid { get; init; }
}