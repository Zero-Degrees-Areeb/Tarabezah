using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;
using Tarabezah.Application.Common.Validation;
using Tarabezah.Application.Common;

namespace Tarabezah.Application.Queries.GetReservationsByDateAndShift;

/// <summary>
/// Handler for retrieving and grouping reservations by date and shift
/// </summary>
public class GetReservationsByDateAndShiftQueryHandler : IRequestHandler<GetReservationsByDateAndShiftQuery, PaginatedResponseDto<ReservationGroupsResponseDto>>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<RestaurantShift> _restaurantShiftRepository;
    private readonly IFloorplanElementRepository _floorplanElementRepository;
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly IRepository<CombinedTableMember> _combinedTableMemberRepository;
    private readonly RestaurantShiftValidator _validator;
    private readonly ILogger<GetReservationsByDateAndShiftQueryHandler> _logger;
    private readonly TimeZoneInfo _jordanTimeZone;

    public GetReservationsByDateAndShiftQueryHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<Shift> shiftRepository,
        IRepository<RestaurantShift> restaurantShiftRepository,
        IFloorplanElementRepository floorplanElementRepository,
        IFloorplanRepository floorplanRepository,
        IRepository<CombinedTableMember> combinedTableMemberRepository,
        ILogger<GetReservationsByDateAndShiftQueryHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _restaurantRepository = restaurantRepository;
        _shiftRepository = shiftRepository;
        _restaurantShiftRepository = restaurantShiftRepository;
        _floorplanElementRepository = floorplanElementRepository;
        _floorplanRepository = floorplanRepository;
        _combinedTableMemberRepository = combinedTableMemberRepository;
        _validator = new RestaurantShiftValidator(restaurantRepository, shiftRepository, restaurantShiftRepository, logger);
        _logger = logger;
        _jordanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
    }

    public async Task<PaginatedResponseDto<ReservationGroupsResponseDto>> Handle(
        GetReservationsByDateAndShiftQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Getting reservations for restaurant {RestaurantGuid}, date {Date} and shift {Shift}, page {Page} with size {Size}",
                request.RestaurantGuid,
                request.ReservationDate.Date,
                request.ShiftName,
                request.PageNumber,
                request.PageSize);

            // Convert all times to Jordan timezone
            var jordanCurrentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _jordanTimeZone);
            var reservationDateTime = TimeZoneInfo.ConvertTime(request.ReservationDate, _jordanTimeZone);

            _logger.LogDebug("Current time in Jordan: {JordanTime}, Reservation time in Jordan: {ReservationTime}",
                jordanCurrentTime,
                reservationDateTime);

            // Remove the past date validation since we want to show past records
            // if (reservationDateTime.Date < jordanCurrentTime.Date)
            // {
            //     _logger.LogWarning("Requested reservation date {ReservationDate} is in the past (Jordan time: {JordanTime})",
            //         request.ReservationDate, jordanCurrentTime);
            //     throw new ArgumentException("Cannot get reservations for past dates");
            // }

            var (restaurant, shift) = await _validator.ValidateRestaurantAndShift(request.RestaurantGuid, request.ShiftName);

            // Get all reservations for the specified date and shift with included relations
            var reservations = await _reservationRepository.GetAllWithIncludesAsync(
                includes: new[] { "Client" });

            var dateOnly = request.ReservationDate.Date;
            var dateReservations = reservations
                .Where(r => r.Date == dateOnly && r.ShiftId == shift.Id && r.Status.HasValue)
                .ToList();

            _logger.LogDebug("Found {Count} total reservations for date {Date} and shift {Shift}",
                dateReservations.Count, dateOnly, shift.Name);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(request.SearchName))
            {
                var searchTerm = request.SearchName.ToLower();
                dateReservations = dateReservations
                    .Where(r => (r.Client?.Name?.ToLower().Contains(searchTerm) ?? false) ||
                                (r.Client?.PhoneNumber?.ToLower().Contains(searchTerm) ?? false))
                    .ToList();

                _logger.LogDebug("Applied search filter '{SearchTerm}', found {Count} matching reservations",
                    request.SearchName, dateReservations.Count);
            }

            // Apply tags filter if provided
            if (request.Tags != null && request.Tags.Any())
            {
                dateReservations = dateReservations
                    .Where(r => r.Tags != null && request.Tags.All(tag => r.Tags.Contains(tag)))
                    .ToList();

                _logger.LogDebug("Applied tags filter, found {Count} matching reservations",
                    dateReservations.Count);
            }

            // Apply party size filter if provided
            if (request.MinPartySize.HasValue)
            {
                dateReservations = dateReservations
                    .Where(r => r.PartySize >= request.MinPartySize.Value)
                    .ToList();

                _logger.LogDebug("Applied min party size filter {MinPartySize}, found {Count} matching reservations",
                    request.MinPartySize.Value, dateReservations.Count);
            }

            if (request.MaxPartySize.HasValue)
            {
                dateReservations = dateReservations
                    .Where(r => r.PartySize <= request.MaxPartySize.Value)
                    .ToList();

                _logger.LogDebug("Applied max party size filter {MaxPartySize}, found {Count} matching reservations",
                    request.MaxPartySize.Value, dateReservations.Count);
            }

            // Apply time range filters if provided, using Jordan time
            if (request.StartTime.HasValue)
            {
                var startTimeInJordan = request.StartTime.Value;
                dateReservations = dateReservations
                    .Where(r => r.Time >= startTimeInJordan)
                    .ToList();

                _logger.LogDebug("Applied start time filter {StartTime} (Jordan time), found {Count} matching reservations",
                    startTimeInJordan, dateReservations.Count);
            }

            if (request.EndTime.HasValue)
            {
                var endTimeInJordan = request.EndTime.Value;
                dateReservations = dateReservations
                    .Where(r => r.Time <= endTimeInJordan)
                    .ToList();

                _logger.LogDebug("Applied end time filter {EndTime} (Jordan time), found {Count} matching reservations",
                    endTimeInJordan, dateReservations.Count);
            }

            // Apply status filter if provided
            if (request.Statuses != null && request.Statuses.Any())
            {
                dateReservations = dateReservations
                    .Where(r => r.Status.HasValue && request.Statuses.Contains(r.Status.Value))
                    .ToList();

                _logger.LogDebug("Applied status filter {Statuses}, found {Count} matching reservations",
                    string.Join(", ", request.Statuses), dateReservations.Count);
            }
            else
            {
                // If no status filter provided, only show reservations with non-null status
                dateReservations = dateReservations
                    .Where(r => r.Status.HasValue)
                    .ToList();

                _logger.LogDebug("Filtered to show only non-null status reservations, found {Count} reservations",
                    dateReservations.Count);
            }

            // Apply sorting if provided
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                switch (request.SortBy.ToLower())
                {
                    case "clientname":
                        dateReservations = dateReservations
                            .OrderBy(r => r.Client?.Name ?? "Walk-in Guest")
                            .ToList();
                        _logger.LogDebug("Applied sorting by client name");
                        break;
                    case "time":
                        dateReservations = dateReservations
                            .OrderBy(r => r.Time)
                            .ToList();
                        _logger.LogDebug("Applied sorting by time");
                        break;
                    default:
                        _logger.LogWarning("Unknown sort field '{SortBy}', ignoring sort", request.SortBy);
                        break;
                }
            }

            _logger.LogDebug("Found {Count} reservations with non-null status for date {Date} and shift {Shift}",
                dateReservations.Count,
                request.ReservationDate.Date,
                shift.Name);

            // Get all combined table memberships
            var combinedTableMembers = await _combinedTableMemberRepository.GetAllWithIncludesAsync(
                includes: new[] { "CombinedTable", "FloorplanElementInstance", "FloorplanElementInstance.Element", "FloorplanElementInstance.Floorplan" });

            var response = new ReservationGroupsResponseDto();

            // Group reservations by status
            var reservationGroups = dateReservations
                .GroupBy(r => r.Status)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Calculate pagination
            var totalCount = dateReservations.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;

            // Create status groups
            foreach (var group in reservationGroups)
            {
                var reservationDetails = new List<ReservationDetailDto>();
                foreach (var reservation in group.Value.Skip(skip).Take(take))
                {
                    // Update late reservation check using Jordan time
                    var isLate = false;
                    if (reservation.Status == ReservationStatus.Upcoming)
                    {
                        var reservationTimeInJordan = new DateTime(
                            reservationDateTime.Year,
                            reservationDateTime.Month,
                            reservationDateTime.Day,
                            reservation.Time.Hours,
                            reservation.Time.Minutes,
                            0,
                            DateTimeKind.Unspecified);

                        var lateThreshold = reservationTimeInJordan.AddMinutes(15);
                        isLate = jordanCurrentTime > lateThreshold;

                        _logger.LogDebug(
                            "Reservation {ReservationId} late status: {IsLate}. Jordan current time: {CurrentTime}, Reservation time: {ReservationTime}, Late threshold: {LateThreshold}",
                            reservation.Id,
                            isLate,
                            jordanCurrentTime,
                            reservationTimeInJordan,
                            lateThreshold);
                    }

                    var reservationDetail = new ReservationDetailDto
                    {
                        Id = reservation.Guid.ToString(),
                        ClientGuid = reservation.Client?.Guid.ToString() ?? "",
                        ClientName = reservation.Client?.Name ?? "Walk-in Guest",
                        ClientTags = GetClientTagsAsStrings(reservation.Client),
                        Time = reservation.Time.ToString(),
                        IsLate = isLate,
                        Status = reservation.Status.ToString(),
                        PartySize = reservation.PartySize,
                        ReservationType = reservation.Type.ToString(),
                        Notes = reservation.Notes ?? string.Empty,
                        ReservationTags = reservation.Tags ?? new List<string>()
                    };

                    // Handle table assignments (either single or combined)
                    if (reservation.ReservedElementId.HasValue)
                    {
                        // Single table assignment
                        var element = await _floorplanElementRepository.GetByIdAsync(reservation.ReservedElementId.Value);
                        if (element != null && element.Element != null)
                        {
                            var floorplan = await _floorplanRepository.GetByIdAsync(element.FloorplanId);
                            _logger.LogDebug(
                                "Processing single table assignment for reservation {ReservationId}, Table: {TableName}",
                                reservation.Id,
                                element.Element.Name);

                            reservationDetail.TableInfo.Add(new TableInfoDto
                            {
                                TableId = element.TableId ?? string.Empty,
                                TableName = element.Element.Name ?? "Unknown Table",
                                ElementGuid = element.Element.Guid,
                                MinCapacity = element.MinCapacity,
                                MaxCapacity = element.MaxCapacity,
                                FloorplanName = floorplan?.Name ?? "Unknown Floorplan",
                                IsCombined = false
                            });
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Single table assignment found but element data is incomplete for reservation {ReservationId}, ElementId: {ElementId}",
                                reservation.Id,
                                reservation.ReservedElementId.Value);
                        }
                    }
                    else if (reservation.CombinedTableMemberId.HasValue)
                    {
                        // Combined table assignment
                        _logger.LogDebug(
                            "Processing combined table assignment for reservation {ReservationId}, CombinedTableMemberId: {MemberId}",
                            reservation.Id,
                            reservation.CombinedTableMemberId.Value);

                        // Find the current member and its combination
                        var currentMember = combinedTableMembers.FirstOrDefault(m => m.Id == reservation.CombinedTableMemberId.Value);
                        if (currentMember != null)
                        {
                            // Get all members of the same combined table
                            var allMembers = combinedTableMembers
                                .Where(m => m.CombinedTableId == currentMember.CombinedTableId)
                                .ToList();

                            if (!allMembers.Any())
                            {
                                _logger.LogWarning(
                                    "No combined table members found for reservation {ReservationId}, CombinedTableId: {CombinedTableId}",
                                    reservation.Id,
                                    currentMember.CombinedTableId);
                                continue;
                            }

                            // Calculate total capacity of all combined tables
                            var totalMinCapacity = allMembers.Sum(m => m.FloorplanElementInstance?.MinCapacity ?? 0);
                            var totalMaxCapacity = allMembers.Sum(m => m.FloorplanElementInstance?.MaxCapacity ?? 0);

                            // Add table info for ALL members of the combination
                            foreach (var member in allMembers)
                            {
                                var element = member.FloorplanElementInstance;
                                if (element?.Element != null && element.Floorplan != null)
                                {
                                    _logger.LogDebug(
                                        "Adding combined table member info: Table {TableName} in Floorplan {FloorplanName}, CombinedTableMember: {MemberGuid}",
                                        element.Element.Name,
                                        element.Floorplan.Name,
                                        member.Guid);

                                    reservationDetail.TableInfo.Add(new TableInfoDto
                                    {
                                        TableId = element.TableId ?? string.Empty,
                                        TableName = element.Element.Name ?? "Unknown Table",
                                        ElementGuid = element.Element.Guid,
                                        MinCapacity = totalMinCapacity, // Use total capacity for all members
                                        MaxCapacity = totalMaxCapacity, // Use total capacity for all members
                                        FloorplanName = element.Floorplan.Name ?? "Unknown Floorplan",
                                        IsCombined = true,
                                        CombinedTableId = member.CombinedTable.Guid // Use the parent CombinedTable's Guid
                                    });
                                }
                                else
                                {
                                    _logger.LogWarning(
                                        "Incomplete element data for combined table member in reservation {ReservationId}, MemberId: {MemberId}",
                                        reservation.Id,
                                        member.Id);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Combined table member not found for reservation {ReservationId}, MemberId: {MemberId}",
                                reservation.Id,
                                reservation.CombinedTableMemberId.Value);
                        }
                    }
                    else
                    {
                        _logger.LogDebug(
                            "No table assignment found for reservation {ReservationId}",
                            reservation.Id);
                    }

                    reservationDetails.Add(reservationDetail);
                }

                if (reservationDetails.Any())
                {
                    response.StatusGroup.Add(new StatusGroupDto
                    {
                        ReservationStatus = group.Key.ToString(),
                        ReservationCount = group.Value.Count,
                        ReservationPartyCount = group.Value.Sum(r => r.PartySize),
                        Reservations = reservationDetails
                    });
                }
            }

            // Set pagination info
            response.PageNumber = request.PageNumber;
            response.PageSize = request.PageSize;
            response.TotalCount = totalCount;
            response.TotalPages = totalPages;
            response.HasPrevious = request.PageNumber > 1;
            response.HasNext = request.PageNumber < totalPages;

            _logger.LogInformation(
                "Found {Count} total reservations across {GroupCount} status groups, page {Page} of {TotalPages}",
                totalCount,
                response.StatusGroup.Count,
                request.PageNumber,
                totalPages);

            return new PaginatedResponseDto<ReservationGroupsResponseDto>
            {
                Data = response,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling the query");
            throw;
        }
    }

    private List<string> GetClientTagsAsStrings(Client? client)
    {
        if (client == null)
        {
            return new List<string>();
        }

        return client.Tags.Select(t => t.ToString()).ToList();
    }
}