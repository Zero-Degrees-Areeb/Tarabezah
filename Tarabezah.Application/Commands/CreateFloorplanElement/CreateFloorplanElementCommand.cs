using MediatR;

namespace Tarabezah.Application.Commands.CreateFloorplanElement;

public record CreateFloorplanElementCommand(
    Guid FloorplanGuid,
    string TableId,
    Guid ElementGuid,
    int MinCapacity,
    int MaxCapacity,
    int X,
    int Y,
    int Height,
    int Width,
    int Rotation) : IRequest<Guid>; 