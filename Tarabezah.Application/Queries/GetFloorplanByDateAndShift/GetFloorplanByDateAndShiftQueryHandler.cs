using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;
using Tarabezah.Application.Common.Validation;
using Tarabezah.Application.Dtos.Floorplans;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Application.Queries.GetFloorplanByDateAndShift;

/// <summary>
/// Handler for retrieving floorplan information by date and shift
/// </summary>
public class GetFloorplanByDateAndShiftQueryHandler : IRequestHandler<GetFloorplanByDateAndShiftQuery, FloorplanByDateShiftResponseDto>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<RestaurantShift> _restaurantShiftRepository;
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly IRepository<BlockTable> _blockTableRepository;
    private readonly IRepository<CombinedTableMember> _combinedTableMemberRepository;
    private readonly RestaurantShiftValidator _validator;
    private readonly ILogger<GetFloorplanByDateAndShiftQueryHandler> _logger;
    private readonly TimeZoneInfo _jordanTimeZone;

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

    public GetFloorplanByDateAndShiftQueryHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<Shift> shiftRepository,
        IRepository<RestaurantShift> restaurantShiftRepository,
        IFloorplanRepository floorplanRepository,
        IRepository<BlockTable> blockTableRepository,
        IRepository<CombinedTableMember> combinedTableMemberRepository,
        ILogger<GetFloorplanByDateAndShiftQueryHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _restaurantRepository = restaurantRepository;
        _shiftRepository = shiftRepository;
        _restaurantShiftRepository = restaurantShiftRepository;
        _floorplanRepository = floorplanRepository;
        _blockTableRepository = blockTableRepository;
        _combinedTableMemberRepository = combinedTableMemberRepository;
        _validator = new RestaurantShiftValidator(restaurantRepository, shiftRepository, restaurantShiftRepository, logger);
        _logger = logger;
        _jordanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
    }

    public async Task<FloorplanByDateShiftResponseDto> Handle(
        GetFloorplanByDateAndShiftQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Getting floorplans for restaurant {RestaurantGuid}, date {Date} and shift {Shift}",
                request.RestaurantGuid,
                request.ReservationDate,
                request.ShiftName);

            var (restaurant, shift) = await _validator.ValidateRestaurantAndShift(request.RestaurantGuid, request.ShiftName);

            var floorplans = await _floorplanRepository.GetByRestaurantIdAsync(restaurant.Id);
            if (!floorplans.Any())
            {
                _logger.LogWarning("No floorplans found for restaurant {RestaurantName}", restaurant.Name);
                throw new ArgumentException($"No floorplans found for restaurant {restaurant.Name}");
            }

            var reservations = await _reservationRepository.GetAllWithIncludesAsync(
                includes: new[] { "Client" });
            var dateReservations = reservations
                .Where(r => r.Date.Date == request.ReservationDate.Date
                    && r.ShiftId == shift.Id)
                .ToList();

            _logger.LogDebug("Found {Count} reservations for date {Date} and shift {Shift}",
                dateReservations.Count,
                request.ReservationDate.Date,
                shift.Name);

            var response = new FloorplanByDateShiftResponseDto();

            foreach (var floorplan in floorplans)
            {
                try
                {
                    _logger.LogDebug("Processing floorplan {FloorplanName} ({FloorplanGuid})",
                        floorplan.Name,
                        floorplan.Guid);

                    var floorplanInfo = new FloorplanInfo
                    {
                        FloorplanGuid = floorplan.Guid,
                        FloorplanName = floorplan.Name ?? "Unnamed Floorplan"
                    };

                    foreach (var element in floorplan.Elements)
                    {
                        try
                        {
                            var elementDto = await CreateElementDto(element, dateReservations, request.ReservationDate, shift);
                            floorplanInfo.Elements.Add(elementDto);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Error processing element {ElementGuid} in floorplan {FloorplanGuid}",
                                element.Guid,
                                floorplan.Guid);
                            continue;
                        }
                    }

                    response.Floorplans.Add(floorplanInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing floorplan {FloorplanGuid}", floorplan.Guid);
                    continue;
                }
            }

            _logger.LogInformation(
                "Found {FloorplanCount} floorplans with total {ElementCount} elements",
                response.Floorplans.Count,
                response.Floorplans.Sum(f => f.Elements.Count));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving floorplans for restaurant {RestaurantGuid}", request.RestaurantGuid);
            throw;
        }
    }
    private async Task<FloorplanElementDetailDto> CreateElementDto(
        FloorplanElementInstance element,
        IEnumerable<Reservation> reservations,
        DateTime reservationDate,
        Shift shift)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        var elementDto = new FloorplanElementDetailDto
        {
            ElementInstanceGuid = element.Guid,
            ElementTypeGuid = element.Element?.Guid ?? Guid.Empty,
            TableId = element.TableId ?? string.Empty,
            ElementName = element.Element?.Name ?? "Unknown Element",
            ImageUrl = element.Element?.ImageUrl ?? string.Empty,
            ElementType = element.Element?.TableType.ToString() ?? "Unknown",
            MinCapacity = element.MinCapacity,
            MaxCapacity = element.MaxCapacity,
            X = element.X,
            Y = element.Y,
            Height = element.Height,
            Width = element.Width,
            Rotation = element.Rotation,
            IsReservable = false,
            IsReserved = false,
            IsBlocked = false,
            ReservationInfo = new List<ReservationInfoDto>(),
            BlockedTable = null,
            CombinedTableDetails = new List<CombinedTableDetailsDto>(),
            BlockedTables = new List<BlockedTableDto>()
        };

        try
        {
            // Convert current time to Jordan's timezone
            var jordanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
            var currentTimeInJordan = TimeZoneInfo.ConvertTime(DateTime.UtcNow, jordanTimeZone).TimeOfDay;

            _logger.LogDebug("Current Time in Jordan: {0}", FormatTimeToAmPm(currentTimeInJordan));

            // Get all blocked tables for this element on the requested date
            var blockedTables = await _blockTableRepository.GetAllAsync();
            var blockedEntries = blockedTables.Where(bt =>
                bt.FloorplanElementInstanceId == element.Id &&
                bt.StartDate.Date <= reservationDate.Date &&
                bt.EndDate.Date >= reservationDate.Date &&
                bt.StartTime < shift.EndTime &&
                bt.EndTime > shift.StartTime).ToList();

            // Add all blocked entries to the element's BlockedTables list
            foreach (var blocked in blockedEntries)
            {
                elementDto.BlockedTables.Add(new BlockedTableDto
                {
                    Guid = blocked.Guid,
                    StartDate = blocked.StartDate,
                    EndDate = blocked.EndDate,
                    StartTime = blocked.StartTime,
                    EndTime = blocked.EndTime,
                    Notes = blocked.Notes,
                });
            }

            // Still set the single BlockedTable property for backward compatibility
            // Use the current active block if any (where current time is within the block time)
            var currentActiveBlock = blockedEntries.FirstOrDefault(bt =>
                currentTimeInJordan >= bt.StartTime &&
                currentTimeInJordan <= bt.EndTime);

            if (currentActiveBlock != null)
            {
                elementDto.IsBlocked = true;
                elementDto.BlockedTable = new BlockedTableDto
                {
                    Guid = currentActiveBlock.Guid,
                    StartDate = currentActiveBlock.StartDate,
                    EndDate = currentActiveBlock.EndDate,
                    StartTime = currentActiveBlock.StartTime,
                    EndTime = currentActiveBlock.EndTime,
                    Notes = currentActiveBlock.Notes,
                };
            }

            if (element.Element?.Purpose == ElementPurpose.Decorative)
            {
                return elementDto;
            }

            elementDto.IsReservable = true;

            // Get all combined table memberships for this element
            var combinedTableMemberships = await _combinedTableMemberRepository.GetAllWithIncludesAsync(
                new[] { "CombinedTable", "FloorplanElementInstance", "Reservations" });

            // Get memberships for the current element
            var elementMemberships = combinedTableMemberships
                .Where(ctm => ctm.FloorplanElementInstanceId == element.Id)
                .ToList();

            // Get direct reservations for this element
            var directReservations = reservations
                .Where(r => r.ReservedElementId == element.Id &&
                           r.Date.Date == reservationDate.Date &&
                           r.ShiftId == shift.Id)
                .ToList();

            // Get all combination IDs this element is part of
            var combinationIds = elementMemberships.Select(em => em.CombinedTableId).ToList();

            // Get ALL reservations for ANY member of the same combinations
            var combinedReservations = reservations
                .Where(r => r.CombinedTableMemberId.HasValue &&
                           r.Date.Date == reservationDate.Date &&
                           r.ShiftId == shift.Id)
                .Join(
                    combinedTableMemberships.Where(ctm => combinationIds.Contains(ctm.CombinedTableId)),
                    r => r.CombinedTableMemberId.Value,
                    ctm => ctm.Id,
                    (r, ctm) => r)
                .ToList();

            _logger.LogDebug(
                "Found {DirectCount} direct reservations and {CombinedCount} combined reservations for element {ElementId}",
                directReservations.Count,
                combinedReservations.Count,
                element.Id);

            // Add all reservations to the element DTO
            var currentDateTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, jordanTimeZone);

            // Sort reservations by time to get the earliest upcoming reservation first
            var sortedReservations = directReservations.Concat(combinedReservations)
                .OrderBy(r => r.Time)
                .ToList();

            foreach (var reservation in sortedReservations)
            {
                var reservationDateTime = reservationDate.Date.Add(reservation.Time);
                var timeDiff = reservationDateTime - currentDateTime;

                var startTime = reservation.Time;
                var durationInMinutes = reservation.Duration ?? 0;
                var totalMinutes = (startTime.TotalMinutes + durationInMinutes) % (24 * 60);
                var wrappedEndTime = TimeSpan.FromMinutes(totalMinutes);

                // Create reservation info DTO
                var reservationInfo = new ReservationInfoDto
                {
                    ReservationGuid = reservation.Guid,
                    ClientName = reservation.Client?.Name ?? "Walk-in Guest",
                    PartySize = reservation.PartySize,
                    ReservationTime = FormatTimeToAmPm(startTime),
                    EndTime = FormatTimeToAmPm(wrappedEndTime),
                    Status = reservation.Status?.ToString() ?? "Waitlist",
                    Duration = reservation.Duration.HasValue ? $"{reservation.Duration.Value / 60}h.{reservation.Duration.Value % 60}m" : ""
                };

                // Add to general reservation info list
                elementDto.ReservationInfo.Add(reservationInfo);

                // Check if this is a current active reservation
                if (currentTimeInJordan >= startTime && currentTimeInJordan <= wrappedEndTime)
                {
                    // Only mark as reserved if status is Upcoming or Seated
                    if (reservation.Status == ReservationStatus.Upcoming || reservation.Status == ReservationStatus.Seated)
                    {
                        elementDto.IsReserved = true;
                        elementDto.UpcomingOrActiveReservation = reservationInfo;
                        _logger.LogDebug(
                            "Table {TableId} marked as reserved - active reservation from {Start} to {End} with status {Status}",
                            element.TableId,
                            FormatTimeToAmPm(startTime),
                            FormatTimeToAmPm(wrappedEndTime),
                            reservation.Status);
                    }
                }
                // Check if this is an upcoming reservation within 15 minutes
                else if (timeDiff.TotalMinutes >= 0 && timeDiff.TotalMinutes <= 15)
                {
                    // Only mark as reserved if status is Upcoming or Seated
                    if (reservation.Status == ReservationStatus.Upcoming || reservation.Status == ReservationStatus.Seated)
                    {
                        elementDto.IsReserved = true;
                        elementDto.UpcomingOrActiveReservation = reservationInfo;
                        _logger.LogDebug(
                            "Table {TableId} marked as reserved - upcoming reservation in {Minutes} minutes with status {Status}",
                            element.TableId,
                            Math.Round(timeDiff.TotalMinutes, 1),
                            reservation.Status);
                    }
                }
            }

            if (elementDto.IsReserved)
            {
                _logger.LogInformation(
                    "Table {TableId} is currently reserved",
                    element.TableId);
            }

            // Process combined table details
            foreach (var membership in elementMemberships)
            {
                var combinedTable = membership.CombinedTable;

                // Get all members of this combination
                var allMembers = combinedTableMemberships
                    .Where(ctm => ctm.CombinedTableId == combinedTable.Id)
                    .ToList();

                // Get all member tables
                var memberTables = allMembers.Select(m => new CombinedTableMembersDto
                {
                    TableId = m.FloorplanElementInstance.TableId ?? "Unknown",
                    MinCapacity = m.FloorplanElementInstance.MinCapacity,
                    MaxCapacity = m.FloorplanElementInstance.MaxCapacity
                }).ToList();

                // Check if any member has an active reservation
                var hasCurrentOverlap = allMembers.Any(member =>
                {
                    // Check direct reservations
                    var memberDirectReservations = reservations.Where(r =>
                        r.ReservedElementId == member.FloorplanElementInstanceId &&
                        r.Date.Date == reservationDate.Date &&
                        r.ShiftId == shift.Id);

                    // Check combined reservations - get reservations for ANY member of this combination
                    var memberCombinedReservations = reservations.Where(r =>
                        r.CombinedTableMemberId.HasValue &&
                        allMembers.Any(m => m.Id == r.CombinedTableMemberId.Value) &&
                        r.Date.Date == reservationDate.Date &&
                        r.ShiftId == shift.Id);

                    return memberDirectReservations.Concat(memberCombinedReservations).Any(r =>
                    {
                        var reservationStartTime = r.Time;
                        var reservationEndTime = r.Time.Add(TimeSpan.FromMinutes(r.Duration ?? 0));
                        return currentTimeInJordan >= reservationStartTime && currentTimeInJordan <= reservationEndTime;
                    });
                });

                // Set the HasCombination flag - combination is available if no overlapping reservations
                var hasCombination = !hasCurrentOverlap;

                elementDto.CombinedTableDetails.Add(new CombinedTableDetailsDto
                {
                    CombinedTableGuid = membership.Guid,
                    CombinedTableName = combinedTable.GroupName ?? "Unnamed Combined Table",
                    TotalMinCapacity = combinedTable.MinCapacity ?? 0,
                    TotalMaxCapacity = combinedTable.MaxCapacity ?? 0,
                    MemberTables = memberTables,
                    HasCombination = hasCombination
                });
            }

            return elementDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating element DTO for element {ElementId}", element.Id);
            throw;
        }
    }
}