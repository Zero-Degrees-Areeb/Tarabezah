using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetRestaurantFloorplansWithElements;

/// <summary>
/// Handler for retrieving all floorplans with their elements for a specific restaurant
/// </summary>
public class GetRestaurantFloorplansWithElementsQueryHandler 
    : IRequestHandler<GetRestaurantFloorplansWithElementsQuery, IEnumerable<FloorplanWithElementsDto>?>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly ILogger<GetRestaurantFloorplansWithElementsQueryHandler> _logger;

    public GetRestaurantFloorplansWithElementsQueryHandler(
        IRestaurantRepository restaurantRepository,
        IFloorplanRepository floorplanRepository,
        ILogger<GetRestaurantFloorplansWithElementsQueryHandler> logger)
    {
        _restaurantRepository = restaurantRepository;
        _floorplanRepository = floorplanRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<FloorplanWithElementsDto>?> Handle(
        GetRestaurantFloorplansWithElementsQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving floorplans with elements for restaurant with GUID {RestaurantGuid}", 
            request.RestaurantGuid);
        
        // Verify the restaurant exists
        var restaurant = await _restaurantRepository.GetByGuidAsync(request.RestaurantGuid);
        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant with GUID {RestaurantGuid} not found", request.RestaurantGuid);
            return null;
        }
        
        // Get all floorplans for the restaurant with their elements
        var floorplans = await _floorplanRepository.GetFloorplansByRestaurantGuidAsync(request.RestaurantGuid);
        
        var result = new List<FloorplanWithElementsDto>();
        
        foreach (var floorplan in floorplans)
        {
            // For each floorplan, get detailed information with elements
            var floorplanWithElements = await _floorplanRepository.GetFloorplanWithElementsByGuidAsync(floorplan.Guid);
            
            if (floorplanWithElements == null)
            {
                continue;
            }
            
            var floorplanDto = new FloorplanWithElementsDto
            {
                Guid = floorplanWithElements.Guid,
                Name = floorplanWithElements.Name,
                CreatedDate = floorplanWithElements.CreatedDate,
                ModifiedDate = floorplanWithElements.ModifiedDate,
                Elements = floorplanWithElements.Elements.Select(e => new FloorplanElementDetailDto
                {
                    Guid = e.Guid,
                    TableId = e.TableId,
                    ElementGuid = e.Element.Guid,
                    ElementName = e.Element.Name,
                    ElementImageUrl = e.Element.ImageUrl,
                    ElementType = e.Element.TableType.ToString(),
                    MinCapacity = e.MinCapacity,
                    MaxCapacity = e.MaxCapacity,
                    X = e.X,
                    Y = e.Y,
                    Height = e.Height,
                    Width = e.Width,
                    Rotation = e.Rotation,
                    Purpose = e.Element.Purpose.ToString(),
                }).ToList()
            };
            
            result.Add(floorplanDto);
        }
        
        _logger.LogInformation("Retrieved {Count} floorplans with elements for restaurant {RestaurantName}", 
            result.Count, restaurant.Name);
        
        return result;
    }
} 