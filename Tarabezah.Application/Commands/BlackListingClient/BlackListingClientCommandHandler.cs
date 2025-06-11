using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Application.Dtos.Clients;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.BlackListingClient;

/// <summary>
/// Handler for blocking/unblocking a client at a restaurant
/// </summary>
public class BlackListingClientCommandHandler : IRequestHandler<BlackListngClientCommand, BlackListDto>
{
    private readonly IRepository<Client> _clientRepository;
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly IRepository<BlackList> _blockingClientRepository;
    private readonly ILogger<BlackListingClientCommandHandler> _logger;

    public BlackListingClientCommandHandler(
        IRepository<Client> clientRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<BlackList> blockingClientRepository,
        ILogger<BlackListingClientCommandHandler> logger)
    {
        _clientRepository = clientRepository;
        _restaurantRepository = restaurantRepository;
        _blockingClientRepository = blockingClientRepository;
        _logger = logger;
    }

    public async Task<BlackListDto> Handle(BlackListngClientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing request to {Action} client {ClientGuid} at restaurant {RestaurantGuid}",
            request.IsBlocked ? "block" : "unblock", request.ClientGuid, request.RestaurantGuid);

        // Get the client
        var client = await _clientRepository.GetByGuidAsync(request.ClientGuid);
        if (client == null)
        {
            _logger.LogWarning("Client {ClientGuid} not found", request.ClientGuid);
            throw new ArgumentException($"Client with GUID {request.ClientGuid} not found");
        }

        // Get the restaurant
        var restaurant = await _restaurantRepository.GetByGuidAsync(request.RestaurantGuid);
        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant {RestaurantGuid} not found", request.RestaurantGuid);
            throw new ArgumentException($"Restaurant with GUID {request.RestaurantGuid} not found");
        }

        // Get existing blocking record if any
        var existingBlock = (await _blockingClientRepository.GetAllAsync())
            .FirstOrDefault(b => b.ClientId == client.Id && b.RestaurantId == restaurant.Id);

        BlackList? blockingClient = null;

        if (request.IsBlocked)
        {
            if (existingBlock != null)
            {
                _logger.LogInformation("Client {ClientGuid} is already blocked at restaurant {RestaurantGuid}",
                    request.ClientGuid, request.RestaurantGuid);
                return MapToDto(existingBlock, client, restaurant);
            }

            // Create new blocking record
            blockingClient = new BlackList
            {
                Guid = Guid.NewGuid(),
                ClientId = client.Id,
                RestaurantId = restaurant.Id,
                Reason = request.Reason,
                BlockedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            await _blockingClientRepository.AddAsync(blockingClient);
            _logger.LogInformation("Successfully blocked client {ClientGuid} at restaurant {RestaurantGuid}",
                request.ClientGuid, request.RestaurantGuid);
        }
        else
        {
            if (existingBlock == null)
            {
                _logger.LogInformation("Client {ClientGuid} is not blocked at restaurant {RestaurantGuid}",
                    request.ClientGuid, request.RestaurantGuid);
                throw new InvalidOperationException($"Client {request.ClientGuid} is not blocked at restaurant {request.RestaurantGuid}");
            }

            await _blockingClientRepository.DeleteAsync(existingBlock);
            _logger.LogInformation("Successfully unblocked client {ClientGuid} at restaurant {RestaurantGuid}",
                request.ClientGuid, request.RestaurantGuid);

            blockingClient = existingBlock;
        }

        return MapToDto(blockingClient, client, restaurant);
    }

    private static BlackListDto MapToDto(BlackList blockingClient, Client client, Restaurant restaurant)
    {
        return new BlackListDto
        {
            Guid = blockingClient.Guid,
            Client = new BlackListedDto
            {
                Guid = client.Guid,
                Name = client.Name,
                PhoneNumber = client.PhoneNumber,
                Email = client.Email,
                Birthday = client.Birthday,
                Source = client.Source.ToString(),
                Tags = client.Tags,
                Notes = client.Notes
            },
            Restaurant = new RestaurantResponseDto
            {
                Guid = restaurant.Guid,
                Name = restaurant.Name,
                CreatedDate = restaurant.CreatedDate
            },
            Reason = blockingClient.Reason,
            BlockedDate = blockingClient.BlockedDate
        };
    }
}