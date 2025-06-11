using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.CreateRestaurant;

public class CreateRestaurantCommandHandler : IRequestHandler<CreateRestaurantCommand, Guid>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ILogger<CreateRestaurantCommandHandler> _logger;

    public CreateRestaurantCommandHandler(
        IRestaurantRepository restaurantRepository,
        ILogger<CreateRestaurantCommandHandler> logger)
    {
        _restaurantRepository = restaurantRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new restaurant with name {Name}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Restaurant name is required");
            throw new ArgumentException("Restaurant name is required");
        }
        
        var restaurant = new Restaurant
        {
            Name = request.Name
        };
        
        var createdRestaurant = await _restaurantRepository.AddAsync(restaurant);
        
        _logger.LogInformation("Successfully created restaurant with GUID {Guid}", createdRestaurant.Guid);
        
        return createdRestaurant.Guid;
    }
} 