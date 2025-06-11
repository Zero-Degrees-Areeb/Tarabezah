using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetFloorplanElementById;

/// <summary>
/// Query to retrieve a specific element in a floorplan
/// </summary>
public record GetFloorplanElementByIdQuery(Guid FloorplanGuid, Guid ElementGuid) : IRequest<FloorplanElementDetailResponseDto?>; 