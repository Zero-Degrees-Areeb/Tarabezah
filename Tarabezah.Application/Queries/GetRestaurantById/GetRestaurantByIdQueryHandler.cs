using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetRestaurantById;

/// <summary>
/// Handler for retrieving a specific restaurant by ID
/// </summary>
public class GetRestaurantByIdQueryHandler : IRequestHandler<GetRestaurantByIdQuery, RestaurantDetailResponseDto?>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ILogger<GetRestaurantByIdQueryHandler> _logger;

    public GetRestaurantByIdQueryHandler(
        IRestaurantRepository restaurantRepository,
        ILogger<GetRestaurantByIdQueryHandler> logger)
    {
        _restaurantRepository = restaurantRepository;
        _logger = logger;
    }

    public async Task<RestaurantDetailResponseDto?> Handle(GetRestaurantByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving restaurant with GUID {RestaurantGuid}", request.RestaurantGuid);
        
        var restaurant = await _restaurantRepository.GetByGuidAsync(request.RestaurantGuid);
        
        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant with GUID {RestaurantGuid} not found", request.RestaurantGuid);
            return null;
        }
        
        var restaurantDto = new RestaurantDetailResponseDto
        {
            Guid = restaurant.Guid,
            Name = restaurant.Name,
            CreatedDate = restaurant.CreatedDate,
            ModifiedDate = restaurant.ModifiedDate
        };
        
        _logger.LogInformation("Retrieved restaurant {RestaurantName} with GUID {RestaurantGuid}", restaurant.Name, restaurant.Guid);
        
        return restaurantDto;
    }
} 