using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Common;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.CreateReservation;

/// <summary>
/// Handler for the CreateReservationCommand
/// </summary>
public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ReservationDto>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Client> _clientRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<FloorplanElementInstance> _floorplanElementRepository;
    private readonly IRepository<BlockTable> _blockTableRepository;
    private readonly ILogger<CreateReservationCommandHandler> _logger;
    private readonly TimeZoneInfo _jordanTimeZone;

    public CreateReservationCommandHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<Client> clientRepository,
        IRepository<Shift> shiftRepository,
        IRepository<FloorplanElementInstance> floorplanElementRepository,
        IRepository<BlockTable> blockTableRepository,
        TimeZoneInfo jordanTimeZone,
        ILogger<CreateReservationCommandHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _clientRepository = clientRepository;
        _shiftRepository = shiftRepository;
        _floorplanElementRepository = floorplanElementRepository;
        _blockTableRepository = blockTableRepository;
        _jordanTimeZone = jordanTimeZone;
        _logger = logger;
    }

    public async Task<ReservationDto> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating reservation for client {ClientGuid} on {Date} at {Time}",
            request.ClientGuid, request.Date.ToShortDateString(), FormatTimeToAmPm(request.Time));

        // Get the current Jordan time
        var nowUtc = DateTime.UtcNow;
        var nowJordan = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _jordanTimeZone);

        var nowjodenDate = nowJordan.Date;

        if (request.Date.Date < nowJordan.Date)
        {
            _logger.LogError("Cannot book a time in the past time. Date: {Date}, Now: {Now}",
                request.Date, FormatDateTimeToAmPm(nowJordan));
            throw new InvalidOperationException("Cannot book a time in the past time.");
        }
        if (request.Date.Date == nowJordan.Date && request.Time.HasValue && request.Time.Value < nowJordan.TimeOfDay)
        {
            _logger.LogError("Cannot book a time in the past time. Date: {Date}, Time: {Time}, Now: {Now}",
                request.Date, FormatTimeToAmPm(request.Time), FormatDateTimeToAmPm(nowJordan));
            throw new InvalidOperationException("Cannot book a time in the past time.");
        }

        // Convert tag integer values to ClientTag enum values
        var tags = request.Tags != null
            ? EnumCollectionConverter.ToEnumList<ClientTag>(request.Tags)
            : new List<ClientTag>();

        // Convert tags to strings for storage
        var tagStrings = GetClientTagsAsStrings(tags);

        // Find the client by GUID
        var client = await _clientRepository.GetByGuidAsync(request.ClientGuid);
        if (client == null)
        {
            _logger.LogError("Client with GUID {ClientGuid} not found", request.ClientGuid);
            throw new Exception($"Client with GUID {request.ClientGuid} not found");
        }

        // Find the shift by GUID
        var shift = await _shiftRepository.GetByGuidAsync(request.ShiftGuid);
        if (shift == null)
        {
            _logger.LogError("Shift with GUID {ShiftGuid} not found", request.ShiftGuid);
            throw new Exception($"Shift with GUID {request.ShiftGuid} not found");
        }

        // Validate the reservation time against the shift's time range
        if (!request.Time.HasValue || request.Time < shift.StartTime || request.Time > shift.EndTime)
        {
            _logger.LogError("Reservation time {Time} is outside the shift time range {StartTime} - {EndTime}",
                FormatTimeToAmPm(request.Time), FormatTimeToAmPm(shift.StartTime), FormatTimeToAmPm(shift.EndTime));
            throw new Exception($"Reservation time {FormatTimeToAmPm(request.Time)} is outside the shift time range {FormatTimeToAmPm(shift.StartTime)} - {FormatTimeToAmPm(shift.EndTime)}");
        }

        // Allow reservations for current and future shifts
        var shiftStartJordan = request.Date.Date + shift.StartTime;
        var shiftEndJordan = request.Date.Date + shift.EndTime;

        if (request.Date.Date == nowJordan.Date && nowJordan > shiftEndJordan)
        {
            _logger.LogError("Cannot create a reservation for a shift that has already ended on the current date. Current time: {Now}, Shift end: {ShiftEnd}",
                FormatDateTimeToAmPm(nowJordan),
                FormatTimeToAmPm(shift.EndTime));
            throw new Exception("Cannot create a reservation for a shift that has already ended on the current date.");
        }

        // Find the floorplan element if provided
        FloorplanElementInstance? floorplanElement = null;
        if (request.FloorplanElementGuid.HasValue)
        {
            // Fetch the floorplan element with its related Element navigation property
            var floorplanElements = await _floorplanElementRepository.GetAllWithIncludesAsync(new[] { "Element" });

            // Find the specific floorplan element by GUID
            floorplanElement = floorplanElements.FirstOrDefault(e => e.Guid == request.FloorplanElementGuid.Value);

            if (floorplanElement == null)
            {
                _logger.LogError("Floorplan element with GUID {ElementGuid} not found", request.FloorplanElementGuid);
                throw new Exception($"Floorplan element with GUID {request.FloorplanElementGuid} not found");
            }

            // Validate that the element is a reservable table
            if (floorplanElement.Element?.Purpose != ElementPurpose.Reservable)
            {
                _logger.LogError("Floorplan element {ElementGuid} is not a reservable table", request.FloorplanElementGuid);
                throw new Exception($"Floorplan element {request.FloorplanElementGuid} is not a reservable table");
            }

            // Validate table capacity
            if (floorplanElement.MaxCapacity < request.PartySize || floorplanElement.MinCapacity > request.PartySize)
            {
                _logger.LogError(
                    "Party size {PartySize} is not within the allowed range for table {TableId}. Min: {Min}, Max: {Max}",
                    request.PartySize, floorplanElement.TableId, floorplanElement.MinCapacity, floorplanElement.MaxCapacity);
                throw new Exception(
                    $"Party size {request.PartySize} is not within the allowed range for table {floorplanElement.TableId} (Min: {floorplanElement.MinCapacity}, Max: {floorplanElement.MaxCapacity}).");
            }

            // Check if the table is blocked during the reservation time
            var blockedTables = await _blockTableRepository.GetAllWithIncludesAsync(Array.Empty<string>());
            var isBlocked = blockedTables.Any(bt =>
                bt.FloorplanElementInstanceId == floorplanElement.Id &&
                bt.StartDate.Date <= request.Date.Date &&
                bt.EndDate.Date >= request.Date.Date &&
                bt.StartTime < request.Time.Value.Add(TimeSpan.FromMinutes(120)) && // Assuming a default duration of 120 minutes
                bt.EndTime > request.Time.Value);

            if (isBlocked)
            {
                _logger.LogError("Table {TableId} is blocked during the reservation time", floorplanElement.TableId);
                throw new Exception($"Table {floorplanElement.TableId} is blocked during the reservation time.");
            }

            // Check for existing reservations
            var existingReservations = await _reservationRepository.GetAllWithIncludesAsync(new[] { "ReservedElement" });
            var hasConflict = existingReservations.Any(r =>
                r.ReservedElementId == floorplanElement.Id &&
                r.Date.Date == request.Date.Date &&
                r.Time < request.Time.Value.Add(TimeSpan.FromMinutes(120)) && // Assuming a default duration of 120 minutes
                r.Time.Add(TimeSpan.FromMinutes(120)) > request.Time.Value &&
                (r.Status == ReservationStatus.Upcoming || r.Status == ReservationStatus.Seated));

            if (hasConflict)
            {
                _logger.LogError("Table {TableId} already has a reservation during the requested time", floorplanElement.TableId);
                throw new Exception($"Table {floorplanElement.TableId} already has a reservation during the requested time.");
            }
        }

        // Validate and set the reservation status
        ReservationStatus? reservationStatus;
        if (request.FloorplanElementGuid != null)
        {
            // If table is assigned, set to Seated regardless of IsUpcoming
            reservationStatus = ReservationStatus.Seated;
            _logger.LogInformation("Setting reservation status to Seated because table is assigned at {Time}",
                FormatTimeToAmPm(request.Time));
        }
        else
        {
            // If no table assigned, respect the IsUpcoming flag
            reservationStatus = request.IsUpcoming ? ReservationStatus.Upcoming : null;
            _logger.LogInformation("Setting reservation status based on IsUpcoming flag: {Status} at {Time}",
                reservationStatus,
                FormatTimeToAmPm(request.Time));
        }

        // Convert duration string to minutes
        int durationInMinutes = ConvertDurationToMinutes(request.Duration);

        // Create the reservation
        var reservation = new Reservation
        {
            Guid = Guid.NewGuid(),
            ClientId = client.Id,
            ShiftId = shift.Id,
            Date = request.Date.Date, // This ensures time is set to 00:00:00
            Time = request.Time ?? TimeSpan.Zero,
            PartySize = request.PartySize,
            Status = reservationStatus,
            Type = ReservationType.OnCall,
            Tags = tagStrings,
            Notes = request.Notes ?? string.Empty,
            Duration = durationInMinutes,
            ReservedElementId = floorplanElement?.Id,
            CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _jordanTimeZone),
            ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _jordanTimeZone)
        };

        try
        {
            await _reservationRepository.AddAsync(reservation);
            _logger.LogInformation("Successfully created reservation with ID {ReservationId} for client {ClientName} with status {Status} at {Time}",
                reservation.Id,
                client.Name,
                reservation.Status,
                FormatTimeToAmPm(reservation.Time));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create reservation for client {ClientName} at {Time}",
                client.Name,
                FormatTimeToAmPm(request.Time));
            throw;
        }

        // Return the DTO with all related information
        return new ReservationDto
        {
            Guid = reservation.Guid,
            ClientGuid = client.Guid,
            Client = new ClientDto
            {
                Guid = client.Guid,
                Name = client.Name,
                PhoneNumber = client.PhoneNumber,
                Email = client.Email,
                CreatedDate = client.CreatedDate
            },
            ShiftGuid = shift.Guid,
            Shift = new ShiftDto
            {
                Guid = shift.Guid,
                Name = shift.Name,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime
            },
            Date = reservation.Date,
            Time = reservation.Time,
            PartySize = reservation.PartySize,
            Status = reservation.Status,
            Type = reservation.Type,
            Tags = tagStrings,
            Notes = reservation.Notes,
            CreatedDate = reservation.CreatedDate,
            ReservedElementGuid = floorplanElement?.Guid,
            Duration = FormatDuration(durationInMinutes),
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

    private static List<string> GetClientTagsAsStrings(List<ClientTag> tags)
    {
        return tags.Select(t => t.ToString()).ToList();
    }

    /// <summary>
    /// Formats duration from minutes into "Xh:Ym" format
    /// </summary>
    private string FormatDuration(int minutes)
    {
        int hours = minutes / 60;
        int remainingMinutes = minutes % 60;
        return $"{hours}h:{remainingMinutes:D2}m";
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

    private string FormatTimeToAmPm(TimeSpan? time)
    {
        if (!time.HasValue)
            return "No time specified";

        return DateTime.Today.Add(time.Value).ToString("hh:mm tt");
    }

    private string FormatDateTimeToAmPm(DateTime dateTime)
    {
        return dateTime.ToString("MM/dd/yyyy hh:mm tt");
    }
}