using System;
using MediatR;
using Tarabezah.Application.Dtos.Reservations;

namespace Tarabezah.Application.Queries.GetReservationAndCountByDateAndShift;

/// <summary>
/// Query to get reservations and counts grouped by status for a specific restaurant, date and shift
/// </summary>
public record GetReservationAndCountByDateAndShiftQuery : IRequest<ReservationCountDto>
{
    /// <summary>
    /// The GUID of the restaurant to get reservations for
    /// </summary>
    public Guid RestaurantGuid { get; init; }

    /// <summary>
    /// The date to get reservations for
    /// </summary>
    public DateTime ReservationDate { get; init; }

    /// <summary>
    /// The name of the shift to get reservations for
    /// </summary>
    public string ShiftName { get; init; }

    public GetReservationAndCountByDateAndShiftQuery(Guid restaurantGuid, DateTime reservationDate, string shiftName)
    {
        RestaurantGuid = restaurantGuid;
        ReservationDate = reservationDate;
        ShiftName = shiftName;
    }
} 