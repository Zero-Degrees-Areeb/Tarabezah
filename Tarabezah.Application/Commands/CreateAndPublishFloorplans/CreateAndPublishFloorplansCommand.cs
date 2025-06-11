using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Commands.CreateAndPublishFloorplans;

public record CreateFloorplansCommand(
    Guid RestaurantGuid,
    List<CreateFloorplanDto> Floorplans) : IRequest<CreateFloorplansResult>;

public class CreateFloorplanDto
{
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<FloorplanElementDto> Elements { get; set; } = new List<FloorplanElementDto>();
} 