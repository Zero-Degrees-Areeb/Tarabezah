using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetAllRestaurants;

/// <summary>
/// Handler for retrieving all restaurants
/// </summary>
public class GetAllRestaurantsQueryHandler : IRequestHandler<GetAllRestaurantsQuery, IEnumerable<RestaurantResponseDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ILogger<GetAllRestaurantsQueryHandler> _logger;

    public GetAllRestaurantsQueryHandler(
        IRestaurantRepository restaurantRepository,
        ILogger<GetAllRestaurantsQueryHandler> logger)
    {
        _restaurantRepository = restaurantRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<RestaurantResponseDto>> Handle(GetAllRestaurantsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all restaurants");
        
        var restaurants = await _restaurantRepository.GetAllAsync();
        
        var restaurantDtos = restaurants.Select(r => new RestaurantResponseDto
        {
            Guid = r.Guid,
            Name = r.Name,
            CreatedDate = r.CreatedDate
        });
        
        _logger.LogInformation("Retrieved {Count} restaurants", restaurantDtos.Count());
        
        return restaurantDtos;
    }
} 