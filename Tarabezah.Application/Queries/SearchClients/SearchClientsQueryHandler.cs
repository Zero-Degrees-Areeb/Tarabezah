using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.SearchClients;

/// <summary>
/// Handler for the SearchClientsQuery
/// </summary>
public class SearchClientsQueryHandler : IRequestHandler<SearchClientsQuery, IEnumerable<ClientDto>>
{
    private readonly IRepository<Client> _clientRepository;
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<RestaurantShift> _restaurantShiftRepository;
    private readonly IRepository<BlackList> _blockingClientRepository;
    private readonly ILogger<SearchClientsQueryHandler> _logger;

    public SearchClientsQueryHandler(
        IRepository<Client> clientRepository,
        IRepository<Reservation> reservationRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<Shift> shiftRepository,
        IRepository<RestaurantShift> restaurantShiftRepository,
        IRepository<BlackList> blockingClientRepository,
        ILogger<SearchClientsQueryHandler> logger)
    {
        _clientRepository = clientRepository;
        _reservationRepository = reservationRepository;
        _restaurantRepository = restaurantRepository;
        _shiftRepository = shiftRepository;
        _restaurantShiftRepository = restaurantShiftRepository;
        _blockingClientRepository = blockingClientRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ClientDto>> Handle(SearchClientsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching clients with search term: {SearchTerm} for restaurant: {RestaurantGuid}",
            request.SearchTerm, request.RestaurantGuid);

        // Find the restaurant by GUID
        var restaurant = await _restaurantRepository.GetByGuidAsync(request.RestaurantGuid);
        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant with GUID {RestaurantGuid} not found", request.RestaurantGuid);
            return new List<ClientDto>();
        }

        // Get all shifts for this restaurant
        var restaurantShifts = await _restaurantShiftRepository.GetAllAsync();
        var shiftsForRestaurant = restaurantShifts
            .Where(rs => rs.RestaurantId == restaurant.Id)
            .Select(rs => rs.ShiftId)
            .ToList();

        var searchTerm = request.SearchTerm.ToLower();

        // If the search term is empty, return the most recent clients (limited to 20)
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            var recentClients = await _clientRepository.GetAllAsync();

            var recentClientsList = recentClients
                .OrderByDescending(c => c.CreatedDate)
                .Take(20)
                .ToList();

            _logger.LogInformation("Found {Count} recent clients", recentClientsList.Count);

            var dtos = await EnrichWithReservationStatistics(recentClientsList, restaurant.Id, shiftsForRestaurant);
            return dtos;
        }

        // Get all clients from repository
        var allClients = await _clientRepository.GetAllAsync();

        // Filter in memory by name, phone, or email using more flexible search
        var searchTerms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var matchingClients = allClients
            .Where(c =>
                searchTerms.All(term =>
                    (c.Name?.ToLower().Contains(term) ?? false) ||
                    (c.PhoneNumber?.ToLower().Contains(term) ?? false) ||
                    (c.Email?.ToLower().Contains(term) ?? false)))
            .OrderBy(c => c.Name)
            .ToList();

        _logger.LogInformation("Found {Count} matching clients for search terms: {SearchTerms}",
            matchingClients.Count, string.Join(", ", searchTerms));

        var enrichedDtos = await EnrichWithReservationStatistics(matchingClients, restaurant.Id, shiftsForRestaurant);
        return enrichedDtos;
    }

    private async Task<List<ClientDto>> EnrichWithReservationStatistics(
        List<Client> clients,
        int currentRestaurantId,
        List<int> currentRestaurantShiftIds)
    {
        // Get all reservations for these clients
        var allReservations = await _reservationRepository.GetAllAsync();
        var clientDtos = new List<ClientDto>();

        // Get all blocking records for these clients
        var allBlockingRecords = await _blockingClientRepository.GetAllAsync();

        foreach (var client in clients)
        {
            var dto = MapToDto(client);

            if (client.Id != null)
            {
                // Filter reservations for this client
                var clientReservations = allReservations.Where(r => r.ClientId == client.Id).ToList();

                // For current restaurant
                var currentRestaurantReservations = clientReservations
                    .Where(r => currentRestaurantShiftIds.Contains(r.ShiftId))
                    .ToList();

                // For other restaurants
                var otherRestaurantReservations = clientReservations
                    .Where(r => !currentRestaurantShiftIds.Contains(r.ShiftId))
                    .ToList();

                // Completed reservations
                dto.CompletedReservationsCurrentRestaurant = currentRestaurantReservations
                    .Count(r => r.Status == ReservationStatus.Completed);

                dto.CompletedReservationsOtherRestaurants = otherRestaurantReservations
                    .Count(r => r.Status == ReservationStatus.Completed);

                // No-show reservations
                dto.NoShowReservationsCurrentRestaurant = currentRestaurantReservations
                    .Count(r => r.Status == ReservationStatus.NoShow);

                dto.NoShowReservationsOtherRestaurants = otherRestaurantReservations
                    .Count(r => r.Status == ReservationStatus.NoShow);

                // Cancelled reservations
                dto.CancelledReservationsCurrentRestaurant = currentRestaurantReservations
                    .Count(r => r.Status == ReservationStatus.Cancelled);

                dto.CancelledReservationsOtherRestaurants = otherRestaurantReservations
                    .Count(r => r.Status == ReservationStatus.Cancelled);

                // Calculate last visit date (most recent completed reservation at current restaurant)
                var completedReservations = currentRestaurantReservations
                    .Where(r => r.Status == ReservationStatus.Completed)
                    .ToList();

                if (completedReservations.Any())
                {
                    // Find the latest completed reservation date
                    var latestReservation = completedReservations
                        .OrderByDescending(r => r.Date.Date.Add(r.Time))
                        .FirstOrDefault();

                    if (latestReservation != null)
                    {
                        dto.LastVisitDate = latestReservation.Date.Date.Add(latestReservation.Time);
                        _logger.LogDebug(
                            "Found last visit date for client {ClientId}: {LastVisitDate}",
                            client.Id,
                            dto.LastVisitDate);
                    }
                }

                // Get blocking information
                var clientBlocks = allBlockingRecords.Where(b => b.ClientId == client.Id).ToList();

                // Check if blocked in current restaurant
                var blockedInCurrentRestaurant = clientBlocks.Any(b => b.RestaurantId == currentRestaurantId);

                // Count blocks in other restaurants
                var totalBlocks = clientBlocks.Count;
                var otherRestaurantBlocks = blockedInCurrentRestaurant ? totalBlocks - 1 : totalBlocks;

                dto.BlackList = new BlackListed
                {
                    Same = blockedInCurrentRestaurant,
                    Others = otherRestaurantBlocks
                };
            }

            clientDtos.Add(dto);
        }

        return clientDtos;
    }

    private static ClientDto MapToDto(Client client)
    {
        return new ClientDto
        {
            Guid = client.Guid,
            Name = client.Name,
            PhoneNumber = client.PhoneNumber,
            Email = client.Email,
            Birthday = client.Birthday,
            Source = client.Source.ToString(),
            Tags = client.Tags,
            Notes = client.Notes,
            CreatedDate = client.CreatedDate,
            BlackList = new BlackListed() // Initialize with default values
        };
    }
}