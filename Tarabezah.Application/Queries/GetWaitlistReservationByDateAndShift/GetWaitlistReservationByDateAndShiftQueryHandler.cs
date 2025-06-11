using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;
using Tarabezah.Application.Common.Validation;
using Tarabezah.Application.Dtos.Reservations;

namespace Tarabezah.Application.Queries.GetWaitlistReservationByDateAndShift;

/// <summary>
/// Handler for retrieving waitlist reservations (reservations with null status) by date and shift
/// </summary>
public class GetWaitlistReservationByDateAndShiftQueryHandler
    : IRequestHandler<GetWaitlistReservationByDateAndShiftQuery, WaitlistReservationResponseDto>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<RestaurantShift> _restaurantShiftRepository;
    private readonly IFloorplanElementRepository _floorplanElementRepository;
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly IRepository<CombinedTableMember> _combinedTableMemberRepository;
    private readonly RestaurantShiftValidator _validator;
    private readonly ILogger<GetWaitlistReservationByDateAndShiftQueryHandler> _logger;

    public GetWaitlistReservationByDateAndShiftQueryHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<Shift> shiftRepository,
        IRepository<RestaurantShift> restaurantShiftRepository,
        IFloorplanElementRepository floorplanElementRepository,
        IFloorplanRepository floorplanRepository,
        IRepository<CombinedTableMember> combinedTableMemberRepository,
        ILogger<GetWaitlistReservationByDateAndShiftQueryHandler> logger)
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
    }

    public async Task<WaitlistReservationResponseDto> Handle(
        GetWaitlistReservationByDateAndShiftQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Getting waitlist reservations for restaurant {RestaurantGuid}, date {Date} and shift {Shift}, page {Page} with size {Size}",
                request.RestaurantGuid,
                request.ReservationDate,
                request.ShiftName,
                request.PageNumber,
                request.PageSize);

            var (restaurant, shift) = await _validator.ValidateRestaurantAndShift(request.RestaurantGuid, request.ShiftName);

            // Get all reservations for the specified date and shift with included relations
            var reservations = await _reservationRepository.GetAllWithIncludesAsync(
                includes: new[] { "Client", "Client.BlockedByRestaurants" });

            // Filter for waitlist reservations (null status)
            var dateReservations = reservations
                .Where(r => r.Date.Date == request.ReservationDate.Date
                    && r.ShiftId == shift.Id
                    && r.Status == null) // Only get reservations with null status
                .ToList();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(request.SearchName))
            {
                var searchTerm = request.SearchName.ToLower();
                dateReservations = dateReservations
                    .Where(r => (r.Client?.Name?.ToLower().Contains(searchTerm) ?? false) ||
                                (r.Client?.PhoneNumber?.ToLower().Contains(searchTerm) ?? false))
                    .ToList();
                _logger.LogDebug("Applied search filter '{SearchTerm}', found {Count} matching waitlist reservations", request.SearchName, dateReservations.Count);
            }

            // Apply tags filter if provided
            if (request.Tags != null && request.Tags.Any())
            {
                dateReservations = dateReservations
                    .Where(r => r.Tags != null && request.Tags.All(tag => r.Tags.Contains(tag)))
                    .ToList();
                _logger.LogDebug("Applied tags filter, found {Count} matching waitlist reservations", dateReservations.Count);
            }

            // Apply party size filters if provided
            if (request.MinPartySize.HasValue)
            {
                dateReservations = dateReservations
                    .Where(r => r.PartySize >= request.MinPartySize.Value)
                    .ToList();
                _logger.LogDebug("Applied min party size filter {MinPartySize}, found {Count} matching waitlist reservations", request.MinPartySize.Value, dateReservations.Count);
            }
            if (request.MaxPartySize.HasValue)
            {
                dateReservations = dateReservations
                    .Where(r => r.PartySize <= request.MaxPartySize.Value)
                    .ToList();
                _logger.LogDebug("Applied max party size filter {MaxPartySize}, found {Count} matching waitlist reservations", request.MaxPartySize.Value, dateReservations.Count);
            }

            // Apply time range filters if provided
            if (request.StartTime.HasValue)
            {
                dateReservations = dateReservations
                    .Where(r => r.Time >= request.StartTime.Value)
                    .ToList();
                _logger.LogDebug("Applied start time filter {StartTime}, found {Count} matching waitlist reservations", request.StartTime.Value, dateReservations.Count);
            }
            if (request.EndTime.HasValue)
            {
                dateReservations = dateReservations
                    .Where(r => r.Time <= request.EndTime.Value)
                    .ToList();
                _logger.LogDebug("Applied end time filter {EndTime}, found {Count} matching waitlist reservations", request.EndTime.Value, dateReservations.Count);
            }

            // Apply sorting if provided
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                switch (request.SortBy.ToLower())
                {
                    case "clientname":
                        dateReservations = dateReservations.OrderBy(r => r.Client?.Name ?? "Walk-in Guest").ToList();
                        _logger.LogDebug("Applied sorting by client name");
                        break;
                    case "time":
                        dateReservations = dateReservations.OrderBy(r => r.Time).ToList();
                        _logger.LogDebug("Applied sorting by time");
                        break;
                    default:
                        _logger.LogWarning("Unknown sort field '{SortBy}', ignoring sort", request.SortBy);
                        break;
                }
            }

            _logger.LogDebug(
                "Found {Count} waitlist reservations for date {Date} and shift {Shift}",
                dateReservations.Count,
                request.ReservationDate.Date,
                shift.Name);

            // Calculate pagination
            var totalCount = dateReservations.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;

            // Get all combined table memberships
            var combinedTableMembers = await _combinedTableMemberRepository.GetAllWithIncludesAsync(
                includes: new[] { "CombinedTable", "FloorplanElementInstance", "FloorplanElementInstance.Element", "FloorplanElementInstance.Floorplan" });

            // Create waitlist reservations list
            var waitlistReservations = new List<WaitlistReservationDetailDto>();

            foreach (var reservation in dateReservations.Skip(skip).Take(take))
            {
                bool isBlacklisted = false;
                if (reservation.Client?.BlockedByRestaurants != null)
                {
                    // Check if the client is blacklisted by the current restaurant
                    isBlacklisted = reservation.Client.BlockedByRestaurants
                        .Any(bl => bl.RestaurantId == restaurant.Id);
                    
                    if (isBlacklisted)
                    {
                        _logger.LogDebug("Client {ClientName} (ID: {ClientId}) is blacklisted for restaurant {RestaurantName} (ID: {RestaurantId})", 
                            reservation.Client.Name, reservation.ClientId, restaurant.Name, restaurant.Id);
                    }
                }

                var waitlistReservation = new WaitlistReservationDetailDto
                {
                    Id = reservation.Guid.ToString(),
                    ClientName = reservation.Client?.Name ?? "Walk-in Guest",
                    ClientGuid = reservation.Client?.Guid,
                    IsBlacklisted = isBlacklisted,
                    ClientTags = reservation.Client?.Tags ?? new List<string>(),
                    Time = reservation.Time.ToString(),
                    Status = "Waitlist", // Set a static value "Waitlist" for null status reservations
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
                            "Processing single table assignment for waitlist reservation {ReservationId}, Table: {TableName}",
                            reservation.Id,
                            element.Element.Name);

                        waitlistReservation.TableInfo.Add(new TableInformation
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
                            "Single table assignment found but element data is incomplete for waitlist reservation {ReservationId}, ElementId: {ElementId}",
                            reservation.Id,
                            reservation.ReservedElementId.Value);
                    }
                }
                else if (reservation.CombinedTableMemberId.HasValue)
                {
                    // Combined table assignment
                    _logger.LogDebug(
                        "Processing combined table assignment for waitlist reservation {ReservationId}, CombinedTableMemberId: {MemberId}",
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
                                "No combined table members found for waitlist reservation {ReservationId}, CombinedTableId: {CombinedTableId}",
                                reservation.Id,
                                currentMember.CombinedTableId);
                            continue;
                        }

                        // Calculate total capacity of all combined tables
                        var totalMinCapacity = currentMember.CombinedTable.MinCapacity ?? 0;
                        var totalMaxCapacity = currentMember.CombinedTable.MaxCapacity ?? 0;

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

                                waitlistReservation.TableInfo.Add(new TableInformation
                                {
                                    TableId = element.TableId ?? string.Empty,
                                    TableName = element.Element.Name ?? "Unknown Table",
                                    ElementGuid = element.Element.Guid,
                                    MinCapacity = totalMinCapacity,
                                    MaxCapacity = totalMaxCapacity,
                                    FloorplanName = element.Floorplan.Name ?? "Unknown Floorplan",
                                    IsCombined = true,
                                    CombinedTableId = member.CombinedTable.Guid
                                });
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Incomplete element data for combined table member in waitlist reservation {ReservationId}, MemberId: {MemberId}",
                                    reservation.Id,
                                    member.Id);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Combined table member not found for waitlist reservation {ReservationId}, MemberId: {MemberId}",
                            reservation.Id,
                            reservation.CombinedTableMemberId.Value);
                    }
                }

                waitlistReservations.Add(waitlistReservation);
            }

            var response = new WaitlistReservationResponseDto
            {
                Data = new WaitlistDataDto
                {
                    ReservationStatus = "Waitlist",
                    ReservationCount = totalCount,
                    ReservationPartyCount = dateReservations.Sum(r => r.PartySize),
                    Reservations = waitlistReservations
                },
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPrevious = request.PageNumber > 1,
                HasNext = request.PageNumber < totalPages
            };

            _logger.LogInformation(
                "Found {Count} total waitlist reservations, page {Page} of {TotalPages}",
                totalCount,
                request.PageNumber,
                totalPages);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving waitlist reservations for restaurant {RestaurantGuid}", request.RestaurantGuid);
            throw;
        }
    }
}