using System;
using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetReservationByGuid;

/// <summary>
/// Query to get a reservation by its GUID with all details
/// </summary>
public record GetReservationByGuidQuery : IRequest<ReservationDto>
{
    /// <summary>
    /// The GUID of the reservation to retrieve
    /// </summary>
    public Guid ReservationGuid { get; init; }

    /// <summary>
    /// The GUID of the restaurant to check blacklist status (optional)
    /// </summary>
    public Guid? RestaurantGuid { get; init; }

    public GetReservationByGuidQuery(Guid reservationGuid, Guid? restaurantGuid = null)
    {
        ReservationGuid = reservationGuid;
        RestaurantGuid = restaurantGuid;
    }
}