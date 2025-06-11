using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Repositories;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Entities;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Commands.BlockingTable;

/// <summary>
/// Handler for blocking a table for a specific time period
/// </summary>
public class BlockingTableCommandHandler : IRequestHandler<BlockingTableCommand, BlockTableResponse>
{
    private readonly IRepository<FloorplanElementInstance> _floorplanElementRepository;
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<BlockTable> _blockedTableRepository;
    private readonly TimeZoneInfo _jordanTimeZone;
    private readonly ILogger<BlockingTableCommandHandler> _logger;

    public BlockingTableCommandHandler(
        IRepository<FloorplanElementInstance> floorplanElementRepository,
        IRepository<Reservation> reservationRepository,
        IRepository<BlockTable> blockedTableRepository,
        ILogger<BlockingTableCommandHandler> logger,
        TimeZoneInfo jordanTimeZone)
    {
        _floorplanElementRepository = floorplanElementRepository;
        _reservationRepository = reservationRepository;
        _blockedTableRepository = blockedTableRepository;
        _logger = logger;
        _jordanTimeZone = jordanTimeZone;
    }

    public async Task<BlockTableResponse> Handle(BlockingTableCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing block table request for element {ElementGuid} from {StartDate} {StartTime} to {EndDate} {EndTime}",
            request.FloorplanElementInstanceGuid,
            request.StartDate.ToShortDateString(),
            request.StartTime,
            request.EndDate.ToShortDateString(),
            request.EndTime);

        // Get Jordan's current date and time
        var jordanCurrentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _jordanTimeZone);

        // Validate the floorplan element exists
        var floorplanElement = await _floorplanElementRepository.GetAllWithIncludesAsync(
            includes: new[] { "Element" });
        var element = floorplanElement.FirstOrDefault(e => e.Guid == request.FloorplanElementInstanceGuid);
        if (element == null)
        {
            _logger.LogWarning("Floorplan element with GUID {ElementGuid} not found", request.FloorplanElementInstanceGuid);
            throw new ArgumentException($"Floorplan element with GUID {request.FloorplanElementInstanceGuid} not found");
        }

        // Validate the element is reservable
        if (element.Element?.Purpose != ElementPurpose.Reservable)
        {
            _logger.LogWarning(
                "Cannot block element {ElementGuid} as it is not a reservable table",
                request.FloorplanElementInstanceGuid);
            throw new ArgumentException($"Element {request.FloorplanElementInstanceGuid} is not a reservable table");
        }

        // Validate date and time ranges
        if (request.EndTime <= request.StartTime && request.StartDate == request.EndDate)
        {
            _logger.LogWarning("Invalid time range: End time must be after start time on the same day");
            throw new ArgumentException("End time must be after start time on the same day");
        }

        if (request.EndDate < request.StartDate)
        {
            _logger.LogWarning("Invalid date range: End date must be after or equal to start date");
            throw new ArgumentException("End date must be after or equal to start date");
        }

        if (request.StartDate.Date < jordanCurrentDateTime.Date)
        {
            _logger.LogWarning("Invalid start date: Cannot block table in the past");
            throw new ArgumentException("Cannot block table in the past");
        }

        // Combine StartDate and StartTime, EndDate and EndTime
        var startDateTime = request.StartDate.Date + request.StartTime;
        var endDateTime = request.EndDate.Date + request.EndTime;

        // Validate block time against shifts in Jordan time
        var shifts = await _reservationRepository.GetAllWithIncludesAsync(new[] { "Shift" });
        var isWithinShift = shifts.Any(shift =>
        {
            var shiftStart = request.StartDate.Date + shift.Shift.StartTime;
            var shiftEnd = request.StartDate.Date + shift.Shift.EndTime;

            return startDateTime >= shiftStart &&
                   endDateTime <= shiftEnd;
        });

        if (!isWithinShift)
        {
            _logger.LogWarning(
                "Block time {StartTime} - {EndTime} does not fall within any active or future shift in Jordan time",
                request.StartTime, request.EndTime);
            throw new ArgumentException($"Block time {request.StartTime} - {request.EndTime} does not fall within any active or future shift in Jordan time");
        }

        // Get all reservations for the date range for this table
        var reservations = await _reservationRepository
            .GetAllWithIncludesAsync(includes: new[] { "ReservedElement" });

        var conflictingReservations = reservations
            .Where(r => r.ReservedElementId == element.Id &&
                       r.Status == ReservationStatus.Upcoming &&
                       r.Date.Date >= request.StartDate.Date &&
                       r.Date.Date <= request.EndDate.Date)
            .ToList();

        // Check for conflicts with existing reservations
        foreach (var reservation in conflictingReservations)
        {
            var reservationTime = reservation.Time;
            var reservationDate = reservation.Date.Date;

            // Skip if reservation is not on the same day
            if (reservationDate < request.StartDate.Date || reservationDate > request.EndDate.Date)
                continue;

            // Check if reservation time falls between block period
            if (reservationTime >= request.StartTime && reservationTime <= request.EndTime)
            {
                _logger.LogWarning(
                    "Cannot block table. Reservation conflict at {ReservationDate} {ReservationTime}",
                    reservationDate.ToShortDateString(),
                    reservationTime.ToString(@"hh\:mm"));
                throw new ArgumentException($"Cannot block table. There is a reservation at {reservationDate.ToShortDateString()} {reservationTime:hh\\:mm}");
            }
        }

        // Create new blocked table record
        var blockedTable = new BlockTable
        {
            Guid = Guid.NewGuid(),
            FloorplanElementInstanceId = element.Id,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            Notes = request.Notes
        };

        await _blockedTableRepository.AddAsync(blockedTable);

        _logger.LogInformation(
            "Successfully blocked table {ElementGuid} with block ID {BlockGuid} from {StartDate} {StartTime} to {EndDate} {EndTime}",
            request.FloorplanElementInstanceGuid,
            blockedTable.Guid,
            blockedTable.StartDate.ToShortDateString(),
            blockedTable.StartTime,
            blockedTable.EndDate.ToShortDateString(),
            blockedTable.EndTime);

        return new BlockTableResponse
        {
            BlockTableGuid = blockedTable.Guid,
            FloorplanElementInstanceGuid = request.FloorplanElementInstanceGuid,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            Notes = request.Notes,
            FloorplanElement = new FloorplanElementResponseDto
            {
                Guid = element.Guid,
                TableId = element.TableId,
                ElementGuid = element.Element?.Guid ?? Guid.Empty,
                ElementName = element.Element?.Name ?? string.Empty,
                ElementType = element.Element?.TableType.ToString(),
                ElementImageUrl = element.Element?.ImageUrl,
                MinCapacity = element.MinCapacity,
                MaxCapacity = element.MaxCapacity,
                X = element.X,
                Y = element.Y,
                Width = element.Width,
                Height = element.Height,
                Rotation = element.Rotation
            }
        };
    }
}