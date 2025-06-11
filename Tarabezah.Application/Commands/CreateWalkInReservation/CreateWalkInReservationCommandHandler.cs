using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.CreateWalkInReservation;

/// <summary>
/// Handler for the CreateWalkInReservationCommand
/// </summary>
public class CreateWalkInReservationCommandHandler : IRequestHandler<CreateWalkInReservationCommand, ReservationDto>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Client> _clientRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<FloorplanElementInstance> _floorplanElementRepository;
    private readonly IRepository<BlockTable> _blockTableRepository; // Add this field
    private readonly ILogger<CreateWalkInReservationCommandHandler> _logger;
    private readonly TimeZoneInfo _jordanTimeZone;

    public CreateWalkInReservationCommandHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<Client> clientRepository,
        IRepository<Shift> shiftRepository,
        IRepository<FloorplanElementInstance> floorplanElementRepository,
        IRepository<BlockTable> blockTableRepository,
        TimeZoneInfo jordanTimeZone,
        ILogger<CreateWalkInReservationCommandHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _clientRepository = clientRepository;
        _shiftRepository = shiftRepository;
        _floorplanElementRepository = floorplanElementRepository;
        _blockTableRepository = blockTableRepository; // Assign the injected repository
        _jordanTimeZone = jordanTimeZone;
        _logger = logger;
    }

    public async Task<ReservationDto> Handle(CreateWalkInReservationCommand request, CancellationToken cancellationToken)
    {
        // Get current time in Jordan
        var nowUtc = DateTime.UtcNow;
        var nowJordan = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _jordanTimeZone);
        _logger.LogInformation("Creating walk-in reservation on {Date} at {Time}",
            nowJordan.ToShortDateString(), nowJordan.ToString("hh:mm tt"));

        // Find the client by GUID if provided
        Client? client = null;

        if (request.ClientGuid.HasValue)
        {
            client = await _clientRepository.GetByGuidAsync(request.ClientGuid.Value);
            if (client == null)
            {
                _logger.LogError("Client with GUID {ClientGuid} not found", request.ClientGuid);
                throw new Exception($"Client with GUID {request.ClientGuid} not found");
            }
        }

        // Find the active or upcoming shift
        var shifts = await _shiftRepository.GetAllAsync();
        var currentOrUpcomingShift = shifts.FirstOrDefault(s =>
            (s.StartTime <= nowJordan.TimeOfDay && s.EndTime > nowJordan.TimeOfDay) || // Current shift
            (s.StartTime > nowJordan.TimeOfDay)); // Upcoming shift

        if (currentOrUpcomingShift == null)
        {
            _logger.LogError("No active or upcoming shift found for current time {Time}",
                nowJordan.ToString("hh:mm tt"));
            throw new Exception("No active or upcoming shift found for the current time");
        }

        // Validate reservation time against the shift
        if (nowJordan.TimeOfDay < currentOrUpcomingShift.StartTime || nowJordan.TimeOfDay >= currentOrUpcomingShift.EndTime)
        {
            _logger.LogError("Reservation time {Time} is outside the shift time range {StartTime} - {EndTime}",
                nowJordan.ToString("hh:mm tt"),
                currentOrUpcomingShift.StartTime.ToString(@"hh\:mm tt"),
                currentOrUpcomingShift.EndTime.ToString(@"hh\:mm tt"));
            throw new Exception($"Reservation time {nowJordan.ToString("hh:mm tt")} is outside the shift time range {currentOrUpcomingShift.StartTime.ToString(@"hh\:mm tt")} - {currentOrUpcomingShift.EndTime.ToString(@"hh\:mm tt")}");
        }

        // Convert tag values to string tags
        var tags = new List<string>();
        if (request.TagValues != null && request.TagValues.Any())
        {
            foreach (var tagValue in request.TagValues)
            {
                if (Enum.IsDefined(typeof(ClientTag), tagValue))
                {
                    var tagName = Enum.GetName(typeof(ClientTag), tagValue);
                    if (tagName != null)
                    {
                        tags.Add(tagName);
                        _logger.LogInformation("Added tag {TagName} to reservation", tagName);
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid tag value {TagValue} provided", tagValue);
                }
            }
        }

        // Find the floorplan element if provided
        FloorplanElementInstance? floorplanElement = null;
        if (request.FloorplanElementGuid.HasValue)
        {
            var floorplanElements = await _floorplanElementRepository.GetAllWithIncludesAsync(new[] { "Element" });
            floorplanElement = floorplanElements.FirstOrDefault(e => e.Guid == request.FloorplanElementGuid.Value);

            if (floorplanElement == null)
            {
                _logger.LogError("Floorplan element with GUID {ElementGuid} not found", request.FloorplanElementGuid);
                throw new Exception($"Table with ID {request.FloorplanElementGuid} not found");
            }

            if (floorplanElement.Element?.Purpose != ElementPurpose.Reservable)
            {
                _logger.LogError("Floorplan element {TableId} is not a reservable table", floorplanElement.TableId);
                throw new Exception($"Table {floorplanElement.TableId} is not a reservable table");
            }

            if (floorplanElement.MaxCapacity < request.PartySize)
            {
                _logger.LogError("Table {ElementGuid} has insufficient capacity for party of {PartySize}",
                    request.FloorplanElementGuid, request.PartySize);
                throw new Exception($"Table has insufficient capacity for party of {request.PartySize}. Maximum capacity is {floorplanElement.MaxCapacity}.");
            }

            var blockedTables = await _blockTableRepository.GetAllWithIncludesAsync(Array.Empty<string>());
            var isBlocked = blockedTables.Any(bt =>
                bt.FloorplanElementInstanceId == floorplanElement.Id &&
                bt.StartDate.Date <= nowJordan.Date &&
                bt.EndDate.Date >= nowJordan.Date &&
                bt.StartTime < nowJordan.TimeOfDay.Add(TimeSpan.FromMinutes(120)) &&
                bt.EndTime > nowJordan.TimeOfDay);

            if (isBlocked)
            {
                _logger.LogError("Table {ElementGuid} is blocked during the reservation time", request.FloorplanElementGuid);
                throw new Exception($"Table {request.FloorplanElementGuid} is blocked during the reservation time.");
            }

            // Check for existing reservations
            var existingReservations = await _reservationRepository.GetAllWithIncludesAsync(new[] { "ReservedElement" });
            var hasConflict = existingReservations.Any(r =>
                r.ReservedElementId == floorplanElement.Id &&
                r.Date.Date == nowJordan.Date &&
                r.Time < nowJordan.TimeOfDay.Add(TimeSpan.FromMinutes(120)) &&
                r.Time.Add(TimeSpan.FromMinutes(r.Duration ?? 0)) > nowJordan.TimeOfDay &&
                (r.Status == ReservationStatus.Upcoming || r.Status == ReservationStatus.Seated));

            if (hasConflict)
            {
                _logger.LogError("Table {TableId} already has a reservation during the requested time", floorplanElement.TableId);
                throw new Exception($"Table {floorplanElement.TableId} already has a reservation during the requested time.");
            }
        }

        // Convert duration string to minutes
        int durationInMinutes = ConvertDurationToMinutes(request.Duration);

        // Create the reservation
        var reservation = new Reservation
        {
            Guid = Guid.NewGuid(),
            ClientId = client?.Id,
            ShiftId = currentOrUpcomingShift.Id,
            Date = nowJordan.Date.Date,
            Time = nowJordan.TimeOfDay,
            PartySize = request.PartySize,
            Status = ReservationStatus.Seated,
            Type = ReservationType.WalkIn,
            Tags = tags,
            Notes = request.Notes ?? string.Empty,
            Duration = durationInMinutes,
            ReservedElementId = floorplanElement?.Id,
            CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _jordanTimeZone),
            ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _jordanTimeZone)
        };

        await _reservationRepository.AddAsync(reservation);

        _logger.LogInformation("Created walk-in reservation with ID {ReservationId} for {ClientInfo} with tags {Tags}",
            reservation.Id,
            client != null ? $"client {client.Name}" : "anonymous customer",
            string.Join(", ", tags));

        return new ReservationDto
        {
            Guid = reservation.Guid,
            ClientGuid = client?.Guid ?? Guid.Empty,
            Client = client != null ? new ClientDto
            {
                Guid = client.Guid,
                Name = client.Name,
                PhoneNumber = client.PhoneNumber,
                Email = client.Email,
                CreatedDate = client.CreatedDate
            } : null,
            ShiftGuid = currentOrUpcomingShift.Guid,
            Shift = new ShiftDto
            {
                Guid = currentOrUpcomingShift.Guid,
                Name = currentOrUpcomingShift.Name,
                StartTime = currentOrUpcomingShift.StartTime,
                EndTime = currentOrUpcomingShift.EndTime
            },
            Date = reservation.Date,
            Time = reservation.Time,
            PartySize = reservation.PartySize,
            Status = reservation.Status,
            Type = reservation.Type,
            Tags = tags,
            Notes = reservation.Notes,
            CreatedDate = reservation.CreatedDate,
            ReservedElementGuid = floorplanElement?.Guid,
            ReservedElement = floorplanElement != null ? new FloorplanElementResponseDto
            {
                Guid = floorplanElement.Guid,
                TableId = floorplanElement.TableId,
                MinCapacity = floorplanElement.MinCapacity,
                MaxCapacity = floorplanElement.MaxCapacity,
                ElementType = floorplanElement.Element?.TableType.ToString() ?? string.Empty,
                ElementName = floorplanElement.Element?.Name ?? string.Empty,
                X = floorplanElement.X,
                Y = floorplanElement.Y,
                Height = floorplanElement.Height,
                Width = floorplanElement.Width,
                Rotation = floorplanElement.Rotation
            } : null
        };
    }

    /// <summary>
    /// Converts a duration string in format "HH:mm" to total minutes
    /// </summary>
    private int ConvertDurationToMinutes(string duration)
    {
        try
        {
            // Try parsing format like "0h:30m" or "2h:30m"
            var hourMinMatch = Regex.Match(duration, @"^(\d+)h:(\d+)m$");
            if (hourMinMatch.Success)
            {
                int hours = int.Parse(hourMinMatch.Groups[1].Value);
                int minutes = int.Parse(hourMinMatch.Groups[2].Value);
                return (hours * 60) + minutes;
            }

            // Try parsing format like "0h" or "2h"
            var hourOnlyMatch = Regex.Match(duration, @"^(\d+)h$");
            if (hourOnlyMatch.Success)
            {
                int hours = int.Parse(hourOnlyMatch.Groups[1].Value);
                return hours * 60;
            }

            // Try parsing format like "30m"
            var minOnlyMatch = Regex.Match(duration, @"^(\d+)m$");
            if (minOnlyMatch.Success)
            {
                int minutes = int.Parse(minOnlyMatch.Groups[1].Value);
                return minutes;
            }

            // Try the strict HH:mm format
            var strictMatch = Regex.Match(duration, @"^(\d{2}):(\d{2})$");
            if (strictMatch.Success)
            {
                int hours = int.Parse(strictMatch.Groups[1].Value);
                int minutes = int.Parse(strictMatch.Groups[2].Value);
                return (hours * 60) + minutes;
            }

            // Try more flexible format that allows 1 or 2 digits for hours
            var flexMatch = Regex.Match(duration, @"^(\d{1,2}):(\d{2})$");
            if (flexMatch.Success)
            {
                int hours = int.Parse(flexMatch.Groups[1].Value);
                int minutes = int.Parse(flexMatch.Groups[2].Value);
                return (hours * 60) + minutes;
            }

            // Try parsing just hours if a single number is provided
            if (int.TryParse(duration, out int justHours))
            {
                return justHours * 60;
            }

            _logger.LogWarning("Invalid duration format: {Duration}. Using default of 60 minutes. Expected formats are: HH:mm, H:mm, Nh:MMm, Nh, or Nm", duration);
            return 60;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing duration: {Duration}. Using default of 60 minutes.", duration);
            return 60;
        }
    }
}