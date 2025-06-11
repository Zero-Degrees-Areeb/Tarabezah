using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetRestaurantFloorplans;

/// <summary>
/// Handler for retrieving all floorplans for a restaurant
/// </summary>
public class GetRestaurantFloorplansQueryHandler : IRequestHandler<GetRestaurantFloorplansQuery, IEnumerable<FloorplanSummaryDto>?>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly ILogger<GetRestaurantFloorplansQueryHandler> _logger;

    public GetRestaurantFloorplansQueryHandler(
        IRestaurantRepository restaurantRepository,
        IFloorplanRepository floorplanRepository,
        ILogger<GetRestaurantFloorplansQueryHandler> logger)
    {
        _restaurantRepository = restaurantRepository;
        _floorplanRepository = floorplanRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<FloorplanSummaryDto>?> Handle(GetRestaurantFloorplansQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving floorplans for restaurant with GUID {RestaurantGuid}", request.RestaurantGuid);
        
        // Verify the restaurant exists
        var restaurant = await _restaurantRepository.GetByGuidAsync(request.RestaurantGuid);
        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant with GUID {RestaurantGuid} not found", request.RestaurantGuid);
            return null;
        }
        
        var floorplans = await _floorplanRepository.GetFloorplansByRestaurantGuidAsync(request.RestaurantGuid);
        
        var floorplanDtos = floorplans.Select(f => new FloorplanSummaryDto
        {
            Guid = f.Guid,
            Name = f.Name,
            CreatedDate = f.CreatedDate
        }).ToList();
        
        _logger.LogInformation("Retrieved {Count} floorplans for restaurant {RestaurantName}", 
            floorplanDtos.Count, restaurant.Name);
        
        return floorplanDtos;
    }
} 