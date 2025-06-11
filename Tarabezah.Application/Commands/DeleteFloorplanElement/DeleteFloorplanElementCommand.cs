using MediatR;

namespace Tarabezah.Application.Commands.DeleteFloorplanElement;

/// <summary>
/// Command to delete a specific element in a floorplan
/// </summary>
public record DeleteFloorplanElementCommand(Guid FloorplanGuid, Guid ElementGuid) : IRequest<bool>; 