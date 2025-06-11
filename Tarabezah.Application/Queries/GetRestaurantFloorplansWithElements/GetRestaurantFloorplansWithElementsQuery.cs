using System.Collections.Generic;
using MediatR;

namespace Tarabezah.Application.Queries.GetRestaurantFloorplansWithElements;

/// <summary>
/// Query to retrieve all floorplans with their elements for a specific restaurant
/// </summary>
public record GetRestaurantFloorplansWithElementsQuery(Guid RestaurantGuid) : IRequest<IEnumerable<FloorplanWithElementsDto>?>;

/// <summary>
/// DTO for floorplan with all its elements
/// </summary>
public class FloorplanWithElementsDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public List<FloorplanElementDetailDto> Elements { get; set; } = new List<FloorplanElementDetailDto>();
}

/// <summary>
/// DTO for floorplan element within a floorplan with detailed information
/// </summary>
public class FloorplanElementDetailDto
{
    public Guid Guid { get; set; }
    public string TableId { get; set; } = string.Empty;
    public Guid ElementGuid { get; set; }
    public string ElementName { get; set; } = string.Empty;
    public string ElementImageUrl { get; set; } = string.Empty;
    public string ElementType { get; set; } = string.Empty;
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } 
    public double Height { get; set; } 
    public string Purpose { get; set; } 
    public double Rotation { get; set; }
} 