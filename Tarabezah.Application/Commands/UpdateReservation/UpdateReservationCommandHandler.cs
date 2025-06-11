using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Common;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;
using System.Text.RegularExpressions;

namespace Tarabezah.Application.Commands.UpdateReservation;

/// <summary>
/// Handler for the UpdateReservationCommand
/// </summary>
public class UpdateReservationCommandHandler : IRequestHandler<UpdateReservationCommand, ReservationDto>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<BlockTable> _blockTableRepository;
    private readonly IRepository<Client> _clientRepository;
    private readonly ILogger<UpdateReservationCommandHandler> _logger;
    private readonly TimeZoneInfo _jordanTimeZone;

    public UpdateReservationCommandHandler(
     IReservationRepository reservationRepository,
     IRepository<Shift> shiftRepository,
     IRepository<BlockTable> blockTableRepository,
     IRepository<Client> clientRepository,
     TimeZoneInfo jordanTimeZone,
     ILogger<UpdateReservationCommandHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _shiftRepository = shiftRepository;
        _blockTableRepository = blockTableRepository;
        _clientRepository = clientRepository;
        _jordanTimeZone = jordanTimeZone;
        _logger = logger;
    }

    public async Task<ReservationDto> Handle(UpdateReservationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating reservation {ReservationGuid} for date {Date} at {Time}",
            request.ReservationGuid,
            request.Date.ToShortDateString(),
            FormatTimeToAmPm(request.Time));

        // Find the reservation by GUID with all related entities
        var reservation = await _reservationRepository.GetByGuidWithDetailsAsync(request.ReservationGuid, cancellationToken);
        if (reservation == null)
        {
            _logger.LogError("Reservation with GUID {ReservationGuid} not found", request.ReservationGuid);
            throw new Exception($"Reservation with GUID {request.ReservationGuid} not found");
        }

        // Get Jordan's current time
        var jordanCurrentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _jordanTimeZone);

        // Only validate past dates, allow future dates
        if (request.Date.Date < jordanCurrentTime.Date)
        {
            _logger.LogError("Cannot update a reservation for a past date. Date: {Date}, Now: {Now}",
                request.Date.ToShortDateString(),
                FormatDateTimeToAmPm(jordanCurrentTime));
            throw new Exception("Cannot update a reservation for a past date.");
        }

        // Find the current or upcoming shift
        var shifts = await _shiftRepository.GetAllAsync();
        var currentOrUpcomingShift = shifts.FirstOrDefault(s =>
            (s.StartTime <= (request.Time ?? TimeSpan.Zero) && s.EndTime >= (request.Time ?? TimeSpan.Zero)) || // Current shift
            (s.StartTime >= (request.Time ?? TimeSpan.Zero))); // Upcoming shift

        if (currentOrUpcomingShift == null)
        {
            _logger.LogError("No active or upcoming shift found for the provided time {Time}",
                FormatTimeToAmPm(request.Time));
            throw new Exception("No active or upcoming shift found for the provided time.");
        }

        // Validate reservation time against the shift
        if (request.Time < currentOrUpcomingShift.StartTime || request.Time > currentOrUpcomingShift.EndTime)
        {
            _logger.LogError("Reservation time {Time} is outside the shift time range {StartTime} - {EndTime}",
                FormatTimeToAmPm(request.Time),
                FormatTimeToAmPm(currentOrUpcomingShift.StartTime),
                FormatTimeToAmPm(currentOrUpcomingShift.EndTime));
            throw new Exception($"Reservation time {FormatTimeToAmPm(request.Time)} is outside the shift time range {FormatTimeToAmPm(currentOrUpcomingShift.StartTime)} - {FormatTimeToAmPm(currentOrUpcomingShift.EndTime)}");
        }

        // Convert duration string to minutes
        int durationInMinutes = ConvertDurationToMinutes(request.Duration);

        // Convert tag integer values to ClientTag enum values
        var tags = request.Tags != null
            ? EnumCollectionConverter.ToEnumList<ClientTag>(request.Tags)
            : new List<ClientTag>();

        // Convert tags to strings for storage
        var tagStrings = GetClientTagsAsStrings(tags);

        // Update client details if ClientGuid is provided
        if (request.ClientGuid.HasValue)
        {
            var client = await _clientRepository.GetByGuidAsync(request.ClientGuid.Value);
            if (client == null)
            {
                _logger.LogError("Client with GUID {ClientGuid} not found", request.ClientGuid);
                throw new Exception($"Client with GUID {request.ClientGuid} not found");
            }

            // Update client name and phone if provided
            if (!string.IsNullOrEmpty(request.ClientName))
            {
                client.Name = request.ClientName;
            }
            if (!string.IsNullOrEmpty(request.ClientPhone))
            {
                client.PhoneNumber = request.ClientPhone;
            }

            await _clientRepository.UpdateAsync(client);
            _logger.LogInformation("Updated client details for {ClientName}", client.Name);
        }

        // Prevent double-booking
        if (reservation.ReservedElementId.HasValue)
        {
            var existingReservations = await _reservationRepository.GetAllWithIncludesAsync(new[] { "ReservedElement" });
            var hasConflict = existingReservations.Any(r =>
                r.Id != reservation.Id &&
                r.ReservedElementId == reservation.ReservedElementId &&
                r.Date == request.Date &&
                r.Time < (request.Time ?? TimeSpan.Zero).Add(TimeSpan.FromMinutes(durationInMinutes)) &&
                r.Time.Add(TimeSpan.FromMinutes(r.Duration ?? 0)) > (request.Time ?? TimeSpan.Zero) &&
                (r.Status == ReservationStatus.Upcoming || r.Status == ReservationStatus.Seated));

            if (hasConflict)
            {
                var tableId = reservation.ReservedElement?.TableId ?? "Unknown";
                _logger.LogError("Table {TableId} is already reserved for the selected time", tableId);
                throw new Exception($"Table {tableId} is already reserved for the selected time.");
            }
        }

        // Check for blocked tables
        if (reservation.ReservedElementId.HasValue)
        {
            var blockedTables = await _blockTableRepository.GetAllWithIncludesAsync(Array.Empty<string>());
            var isBlocked = blockedTables.Any(bt =>
                bt.FloorplanElementInstanceId == reservation.ReservedElementId &&
                bt.StartDate.Date <= request.Date &&
                bt.EndDate.Date >= request.Date &&
                bt.StartTime < (request.Time ?? TimeSpan.Zero).Add(TimeSpan.FromMinutes(durationInMinutes)) &&
                bt.EndTime > (request.Time ?? TimeSpan.Zero));

            if (isBlocked)
            {
                var tableId = reservation.ReservedElement?.TableId ?? "Unknown";
                _logger.LogError("Table {TableId} is blocked during the reservation time", tableId);
                throw new Exception($"Table {tableId} is blocked during the reservation time.");
            }
        }

        // Update the reservation (excluding client details)
        reservation.ShiftId = currentOrUpcomingShift.Id;
        reservation.Date = request.Date.Date; // Store only the date part
        reservation.Time = request.Time ?? TimeSpan.Zero;
        reservation.PartySize = request.PartySize;
        reservation.Tags = tagStrings;
        reservation.Notes = request.Notes ?? string.Empty;
        reservation.Duration = durationInMinutes;
        reservation.ModifiedDate = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation);

        _logger.LogInformation("Successfully Updated reservation with ID {ReservationId} for client {ClientName}",
            reservation.Id, reservation.Client?.Name ?? "Unknown");

        // Return the updated reservation DTO with all details
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
                CreatedDate = reservation.Client.CreatedDate
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
            Tags = reservation.Tags,
            Notes = reservation.Notes,
            Duration = FormatDuration(durationInMinutes),
            CreatedDate = reservation.CreatedDate,
            ReservedElementGuid = reservation.ReservedElement?.Guid,
            ReservedElement = reservation.ReservedElement != null ? new FloorplanElementResponseDto
            {
                Guid = reservation.ReservedElement.Guid,
                TableId = reservation.ReservedElement.TableId,
                ElementGuid = reservation.ReservedElement.Element?.Guid ?? Guid.Empty,
                ElementName = reservation.ReservedElement.Element?.Name ?? string.Empty,
                ElementImageUrl = reservation.ReservedElement.Element?.ImageUrl,
                ElementType = reservation.ReservedElement.Element?.TableType.ToString() ?? string.Empty,
                MinCapacity = reservation.ReservedElement.MinCapacity,
                MaxCapacity = reservation.ReservedElement.MaxCapacity,
                X = reservation.ReservedElement.X,
                Y = reservation.ReservedElement.Y,
                Height = reservation.ReservedElement.Height,
                Width = reservation.ReservedElement.Width,
                Rotation = reservation.ReservedElement.Rotation,
                CreatedDate = reservation.ReservedElement.CreatedDate
            } : null
        };
    }

    private List<string> GetClientTagsAsStrings(List<ClientTag> tags)
    {
        return tags.Select(t => t.ToString()).ToList();
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

    /// <summary>
    /// Formats duration in minutes to "Xh.Ym" format
    /// </summary>
    private string FormatDuration(int minutes)
    {
        int hours = minutes / 60;
        int remainingMinutes = minutes % 60;
        return $"{hours}h:{remainingMinutes:D2}m";
    }

    /// <summary>
    /// Formats a TimeSpan to AM/PM format
    /// </summary>
    private string FormatTimeToAmPm(TimeSpan? time)
    {
        if (!time.HasValue)
            return "No time specified";

        return DateTime.Today.Add(time.Value).ToString("hh:mm tt");
    }

    /// <summary>
    /// Formats a DateTime to date and AM/PM format
    /// </summary>
    private string FormatDateTimeToAmPm(DateTime dateTime)
    {
        return dateTime.ToString("MM/dd/yyyy hh:mm tt");
    }
}