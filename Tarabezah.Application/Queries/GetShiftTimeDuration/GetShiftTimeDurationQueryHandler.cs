using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetShiftTimeDuration;

public class GetShiftTimeDurationQueryHandler : IRequestHandler<GetShiftTimeDurationQuery, List<TimeSlotDto>>
{
    private readonly IRepository<RestaurantShift> _restaurantShiftRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IFloorplanElementRepository _floorplanElementRepository;
    private readonly IRepository<BlockTable> _blockTableRepository;
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly ILogger<GetShiftTimeDurationQueryHandler> _logger;
    private readonly TimeZoneInfo _jordanTimeZone;

    public GetShiftTimeDurationQueryHandler(
        IRepository<RestaurantShift> restaurantShiftRepository,
        IRepository<Shift> shiftRepository,
        IFloorplanElementRepository floorplanElementRepository,
        IRepository<BlockTable> blockTableRepository,
        IRepository<Reservation> reservationRepository,
        TimeZoneInfo jordanTimeZone,
        ILogger<GetShiftTimeDurationQueryHandler> logger)
    {
        _restaurantShiftRepository = restaurantShiftRepository;
        _shiftRepository = shiftRepository; 
        _floorplanElementRepository = floorplanElementRepository;
        _blockTableRepository = blockTableRepository;
        _reservationRepository = reservationRepository;
        _jordanTimeZone = jordanTimeZone;
        _logger = logger;
    }

    public async Task<List<TimeSlotDto>> Handle(GetShiftTimeDurationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting shift time duration for restaurant {RestaurantGuid}, shift {ShiftGuid} with party size {PartySize} for date {Date}",
                request.RestaurantGuid, request.ShiftGuid, request.PartySize, request.Date);

            // Get current time in Jordan
            var currentJordanDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _jordanTimeZone);
            _logger.LogDebug("Current Jordan time: {CurrentTime}", currentJordanDateTime);

            // Get the specific shift
            var shift = await _shiftRepository.GetByGuidAsync(request.ShiftGuid);
            if (shift == null)
            {
                _logger.LogWarning("Shift not found with GUID {ShiftGuid}", request.ShiftGuid);
                throw new InvalidOperationException($"Shift not found with GUID {request.ShiftGuid}");
            }

            // If the requested date is in the past, return empty array
            if (request.Date.Date < currentJordanDateTime.Date)
            {
                _logger.LogInformation("Requested date {Date} is in the past", request.Date);
                return new List<TimeSlotDto>();
            }

            // Verify the shift belongs to the restaurant
            var restaurantShifts = await _restaurantShiftRepository.GetAllWithIncludesAsync(new[] { "Restaurant", "Shift" });
            var isShiftInRestaurant = restaurantShifts.Any(rs =>
                rs.Restaurant.Guid == request.RestaurantGuid &&
                rs.Shift.Guid == request.ShiftGuid);

            if (!isShiftInRestaurant)
            {
                _logger.LogWarning("Shift {ShiftGuid} does not belong to restaurant {RestaurantGuid}",
                    request.ShiftGuid, request.RestaurantGuid);
                throw new InvalidOperationException("Shift does not belong to the specified restaurant");
            }

            // Get all floorplan elements for the restaurant
            var floorplanElements = await _floorplanElementRepository.GetByRestaurantGuidAsync(request.RestaurantGuid);

            // Filter for reservable elements with sufficient capacity for the requested party size
            var reservableElements = floorplanElements
                .Where(fei =>
                    fei.Element?.Purpose == ElementPurpose.Reservable &&
                    fei.MinCapacity <= request.PartySize &&
                    fei.MaxCapacity >= request.PartySize &&
                    fei.Element != null) // Ensure Element is not null
                .ToList();

            _logger.LogInformation("Found {Count} reservable elements suitable for party size {PartySize}, TableType filter: {TableType}",
                reservableElements.Count, request.PartySize, request.TableType ?? "none");

            // Debug log each element's details
            foreach (var element in reservableElements)
            {
                _logger.LogDebug("Element: ID={Id}, TableType={TableType}, MinCap={MinCap}, MaxCap={MaxCap}, Purpose={Purpose}",
                    element.TableId,
                    element.Element?.TableType,
                    element.MinCapacity,
                    element.MaxCapacity,
                    element.Element?.Purpose);
            }

            // Parse table type if provided
            if (!string.IsNullOrEmpty(request.TableType) && request.TableType.ToLower() != "view all")
            {
                if (Enum.TryParse<TableType>(request.TableType, true, out var parsedType))
                {
                    reservableElements = reservableElements
                        .Where(t => t.Element?.TableType == parsedType)
                        .OrderBy(t => t.Element?.TableType.ToString())
                        .ToList();

                    _logger.LogInformation("Filtered to {Count} elements of type {TableType}",
                        reservableElements.Count, request.TableType);
                }
                else
                {
                    _logger.LogWarning("Invalid table type provided: {TableType}", request.TableType);
                    return new List<TimeSlotDto>(); // Return empty list for invalid table type
                }
            }
            else
            {
                // If no specific table type is requested or "view all" is specified, show all tables
                _logger.LogInformation("No specific table type requested, showing all {Count} tables",
                    reservableElements.Count);

                reservableElements = reservableElements
                    .OrderBy(t => t.Element?.TableType.ToString())
                    .ToList();
            }

            // Return an empty list if no suitable tables are found
            if (!reservableElements.Any())
            {
                _logger.LogInformation("No suitable tables found for party size {PartySize} and table type {TableType}",
                    request.PartySize, request.TableType ?? "any");
                return new List<TimeSlotDto>();
            }

            // Get all blocked tables and reservations for the date
            var blockedTables = await _blockTableRepository.GetAllWithIncludesAsync(new[] { "FloorplanElementInstance" });
            var dateBlockedTables = blockedTables
                .Where(bt => bt.StartDate.Date <= request.Date.Date && bt.EndDate.Date >= request.Date.Date)
                .ToList();

            var reservations = await _reservationRepository.GetAllWithIncludesAsync(
                new[] { "ReservedElement", "ReservedElement.Floorplan.Restaurant" });
            var dateReservations = reservations
                .Where(r => r.Date.Date == request.Date.Date && r.Status != ReservationStatus.Cancelled)
                .ToList();

            var timeSlots = new List<TimeSlotDto>();
            var currentTime = shift.StartTime;

            // Generate time slots for the shift
            while (currentTime <= shift.EndTime)
            {
                // For today's date, skip time slots that are in the past
                if (request.Date.Date == currentJordanDateTime.Date)
                {
                    var slotDateTime = request.Date.Date.Add(currentTime);
                    if (slotDateTime <= currentJordanDateTime)
                    {
                        currentTime = currentTime.Add(TimeSpan.FromMinutes(15));
                        continue;
                    }
                }

                var (totalCapacity, occupiedSeats) = GetTableCounts(
                    reservableElements,
                    currentTime,
                    request.RestaurantGuid,
                    request.Date,
                    dateBlockedTables,
                    dateReservations);

                timeSlots.Add(new TimeSlotDto
                {
                    Time = currentTime,
                    TotalPatySizes = totalCapacity,
                    AllocatedTables = occupiedSeats,
                    IsAvailable = true
                });

                currentTime = currentTime.Add(TimeSpan.FromMinutes(15));
            }

            return timeSlots;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shift time duration for restaurant {RestaurantGuid}, shift {ShiftGuid}",
                request.RestaurantGuid, request.ShiftGuid);
            throw;
        }
    }

    private (int TotalCapacity, int OccupiedSeats) GetTableCounts(
        List<FloorplanElementInstance> tables,
        TimeSpan timeSlot,
        Guid restaurantGuid,
        DateTime date,
        IEnumerable<BlockTable> blockedTables,
        IEnumerable<Reservation> reservations)
    {
        // Filter tables that can accommodate the party size
        var suitableTables = tables.Where(t =>
            t.Element?.Purpose == ElementPurpose.Reservable &&
            !IsTableBlockedOrReserved(t, timeSlot, restaurantGuid, date, blockedTables, reservations)).ToList();

        // Calculate total max capacity of suitable tables
        var totalCapacity = suitableTables.Sum(table => table.MaxCapacity);

        // Get current reservations for this time slot, filtered by table type
        var currentReservations = reservations.Where(r =>
            r.Time <= timeSlot &&
            r.Time.Add(TimeSpan.FromMinutes(r.Duration ?? 0)) > timeSlot &&
            // Only count reservations where table exists and matches capacity requirements
            r.ReservedElement != null &&
            r.ReservedElement.MinCapacity <= r.PartySize &&
            r.ReservedElement.MaxCapacity >= r.PartySize &&
            // Only count reservations for tables of the same type as the filtered tables
            tables.Any(t => t.Element?.TableType == r.ReservedElement.Element?.TableType));

        // Calculate total occupied seats from valid reservations
        var occupiedSeats = currentReservations.Sum(r => r.PartySize);

        _logger.LogDebug(
            "Time slot {TimeSlot}: Found {TableCount} suitable tables with total capacity {TotalCapacity}, {OccupiedSeats} seats occupied",
            timeSlot.ToString(@"hh\:mm"),
            suitableTables.Count,
            totalCapacity,
            occupiedSeats);

        return (totalCapacity, occupiedSeats);
    }

    private bool IsTableBlockedOrReserved(
        FloorplanElementInstance table,
        TimeSpan timeSlot,
        Guid restaurantGuid,
        DateTime date,
        IEnumerable<BlockTable> blockedTables,
        IEnumerable<Reservation> reservations)
    {
        // Check if table is blocked
        var isBlocked = blockedTables.Any(bt =>
            bt.FloorplanElementInstanceId == table.Id &&
            bt.StartTime <= timeSlot &&
            bt.EndTime > timeSlot);

        if (isBlocked)
            return true;

        // Check if table is reserved
        var isReserved = reservations.Any(r =>
            r.ReservedElement != null &&
            r.ReservedElement.Id == table.Id &&
            r.ReservedElement.Floorplan.Restaurant.Guid == restaurantGuid &&
            r.Time <= timeSlot &&
            r.Time.Add(TimeSpan.FromMinutes((double)r.Duration)) > timeSlot);

        return isReserved;
    }
}