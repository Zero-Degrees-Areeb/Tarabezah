using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediatR;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;
using System.Linq;

namespace Tarabezah.Application.Queries.GetReservationByGuid;

/// <summary>
/// Handler for retrieving a reservation by its GUID with detailed information
/// </summary>
public class GetReservationByGuidQueryHandler : IRequestHandler<GetReservationByGuidQuery, ReservationDto>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRepository<BlackList> _blackListRepository;
    private readonly ILogger<GetReservationByGuidQueryHandler> _logger;

    public GetReservationByGuidQueryHandler(
        IReservationRepository reservationRepository,
        IRepository<BlackList> blackListRepository,
        ILogger<GetReservationByGuidQueryHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _blackListRepository = blackListRepository;
        _logger = logger;
    }

    public async Task<ReservationDto> Handle(GetReservationByGuidQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting reservation details for GUID: {ReservationGuid}", request.ReservationGuid);

        // Find the reservation by GUID with all related entities
        var reservation = await _reservationRepository.GetByGuidWithDetailsAsync(request.ReservationGuid, cancellationToken);

        if (reservation == null)
        {
            _logger.LogWarning("Reservation with GUID {ReservationGuid} not found", request.ReservationGuid);
            throw new Exception($"Reservation with GUID {request.ReservationGuid} not found");
        }

        _logger.LogInformation("Found reservation for client {ClientName}, date {Date}",
            reservation.Client?.Name ?? "Unknown",
            reservation.Date.ToShortDateString());

        // Get blacklist information if restaurant GUID is provided and client exists
        BlackListed? blackListInfo = null;
        if (request.RestaurantGuid.HasValue && reservation.Client != null)
        {
            var blackListEntries = await _blackListRepository.GetAllWithIncludesAsync(
                includes: new[] { "Restaurant" });

            var totalBlacklists = blackListEntries.Count(b => b.ClientId == reservation.Client.Id);
            var isBlacklistedInCurrentRestaurant = blackListEntries.Any(b =>
                b.ClientId == reservation.Client.Id &&
                b.Restaurant.Guid == request.RestaurantGuid.Value);

            blackListInfo = new BlackListed
            {
                Same = isBlacklistedInCurrentRestaurant,
                Others = isBlacklistedInCurrentRestaurant ? totalBlacklists - 1 : totalBlacklists
            };

            _logger.LogInformation(
                "Client {ClientName} blacklist status - Same: {Same}, Others: {Others}",
                reservation.Client.Name,
                blackListInfo.Same,
                blackListInfo.Others);
        }

        // Map to DTO with all details
        return new ReservationDto
        {
            Guid = reservation.Guid,
            ClientGuid = reservation.Client?.Guid,
            Client = reservation.Client != null ? new ClientDto
            {
                Guid = reservation.Client.Guid,
                Name = reservation.Client.Name,
                PhoneNumber = reservation.Client.PhoneNumber,
                Email = reservation.Client.Email,
                CreatedDate = reservation.Client.CreatedDate,
                BlackList = blackListInfo ?? new BlackListed()
            } : null,
            ShiftGuid = reservation.Shift.Guid,
            Shift = new ShiftDto
            {
                Guid = reservation.Shift.Guid,
                Name = reservation.Shift.Name,
                StartTime = reservation.Shift.StartTime,
                EndTime = reservation.Shift.EndTime
            },
            Date = reservation.Date,
            Time = reservation.Time,
            PartySize = reservation.PartySize,
            Status = reservation.Status,
            Type = reservation.Type,
            Tags = reservation.Tags,
            Notes = reservation.Notes,
            CreatedDate = reservation.CreatedDate,
            ReservedElementGuid = reservation.ReservedElement?.Guid,
            Duration = FormatDuration(reservation.Duration),
            ReservedElement = reservation.ReservedElement != null ? new FloorplanElementResponseDto
            {
                Guid = reservation.ReservedElement.Guid,
                TableId = reservation.ReservedElement.TableId,
                MinCapacity = reservation.ReservedElement.MinCapacity,
                MaxCapacity = reservation.ReservedElement.MaxCapacity
            } : null
        };
    }

    /// <summary>
    /// Formats duration in minutes to "Xh.Ym" format
    /// </summary>
    private string FormatDuration(int? minutes)
    {
        int? hours = minutes / 60;
        int? remainingMinutes = minutes % 60;
        return $"{hours}h.{remainingMinutes:D2}m";
    }
}