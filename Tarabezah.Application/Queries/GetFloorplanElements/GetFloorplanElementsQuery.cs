using System.Collections.Generic;
using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetFloorplanElements;

/// <summary>
/// Query to retrieve all elements for a floorplan
/// </summary>
public record GetFloorplanElementsQuery(Guid FloorplanGuid) : IRequest<IEnumerable<FloorplanElementResponseDto>?>; 