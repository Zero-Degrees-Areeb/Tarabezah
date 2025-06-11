using System.Collections.Generic;
using MediatR;

namespace Tarabezah.Application.Queries.GetRestaurantFloorplans;

/// <summary>
/// Query to retrieve all floorplans for a restaurant
/// </summary>
public record GetRestaurantFloorplansQuery(Guid RestaurantGuid) : IRequest<IEnumerable<FloorplanSummaryDto>?>;

/// <summary>
/// DTO for simplified floorplan information
/// </summary>
public class FloorplanSummaryDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
} 