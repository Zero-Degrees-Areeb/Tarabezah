using MediatR;
using System.Collections.Generic;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Commands.CreateFloorplan;

public record CreateFloorplanCommand(
    string Name,
    Guid RestaurantGuid,
    ICollection<FloorplanElementDto> Elements = null) : IRequest<FloorplanDto>;