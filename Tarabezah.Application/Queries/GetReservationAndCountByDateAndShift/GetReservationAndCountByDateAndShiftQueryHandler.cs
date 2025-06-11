using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;
using Tarabezah.Application.Common.Validation;
using Tarabezah.Application.Dtos.Reservations;

namespace Tarabezah.Application.Queries.GetReservationAndCountByDateAndShift;

/// <summary>
/// Handler for retrieving reservation counts by date and shift
/// </summary>
public class GetReservationAndCountByDateAndShiftQueryHandler : IRequestHandler<GetReservationAndCountByDateAndShiftQuery, ReservationCountDto>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly RestaurantShiftValidator _validator;
    private readonly ILogger<GetReservationAndCountByDateAndShiftQueryHandler> _logger;

    public GetReservationAndCountByDateAndShiftQueryHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<Shift> shiftRepository,
        IRepository<RestaurantShift> restaurantShiftRepository,
        ILogger<GetReservationAndCountByDateAndShiftQueryHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _validator = new RestaurantShiftValidator(restaurantRepository, shiftRepository, restaurantShiftRepository, logger);
        _logger = logger;
    }

    public async Task<ReservationCountDto> Handle(
     GetReservationAndCountByDateAndShiftQuery request,
     CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting reservation counts for restaurant {RestaurantGuid}, date {Date} and shift {Shift}",
            request.RestaurantGuid,
            request.ReservationDate,
            request.ShiftName);

        // Convert ReservationDate to Jordan's timezone
        var jordanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
        var reservationDateInJordan = TimeZoneInfo.ConvertTime(request.ReservationDate, jordanTimeZone);

        _logger.LogDebug("Converted ReservationDate to Jordan's timezone: {ReservationDateInJordan}", reservationDateInJordan);

        var (_, shift) = await _validator.ValidateRestaurantAndShift(request.RestaurantGuid, request.ShiftName);

        // Fetch all reservations
        var reservations = await _reservationRepository.GetAllAsync();

        // Filter reservations based on Jordan's date and shift
        var dateReservations = reservations
            .Where(r => TimeZoneInfo.ConvertTime(r.Date, jordanTimeZone).Date == reservationDateInJordan.Date
                && r.ShiftId == shift.Id)
            .ToList();

        var response = new ReservationCountDto
        {
            ReservationDate = reservationDateInJordan,
            ShiftName = request.ShiftName,
            TotalReservations = dateReservations.Count,
            TotalGuests = dateReservations.Sum(r => r.PartySize)
        };

        // Group reservations by status, treating null as "Waitlist"
        response.ReservationCounts = dateReservations
            .GroupBy(r => r.Status?.ToString() ?? "Waitlist") // Assign "Waitlist" for null statuses
            .Select(g => new ReservationStatusCountDto
            {
                Status = g.Key,
                ReservationCount = g.Count(),
                GuestCount = g.Sum(r => r.PartySize)
            })
            .ToList();

        _logger.LogInformation(
            "Found {Count} reservations with {GuestCount} total guests",
            response.TotalReservations,
            response.TotalGuests);

        return response;
    }
}