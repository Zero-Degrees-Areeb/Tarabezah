using MediatR;

namespace Tarabezah.Application.Commands.UpdateFloorplanElement;

public record UpdateFloorplanElementCommand(
    Guid FloorplanGuid,
    Guid ElementInstanceGuid,
    string TableId,
    int MinCapacity,
    int MaxCapacity,
    int X,
    int Y,
    int Height,
    int Width,
    int Rotation) : IRequest<Guid>; 