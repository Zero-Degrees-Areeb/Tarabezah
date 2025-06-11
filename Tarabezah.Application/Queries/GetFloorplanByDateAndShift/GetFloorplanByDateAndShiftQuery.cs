using System;
using MediatR;
using Tarabezah.Application.Dtos.Floorplans;

namespace Tarabezah.Application.Queries.GetFloorplanByDateAndShift;

/// <summary>
/// Query to get floorplan information for a specific restaurant, date and shift
/// </summary>
public record GetFloorplanByDateAndShiftQuery : IRequest<FloorplanByDateShiftResponseDto>
{
    /// <summary>
    /// The GUID of the restaurant to get the floorplan for
    /// </summary>
    public Guid RestaurantGuid { get; init; }

    /// <summary>
    /// The date to get the floorplan for
    /// </summary>
    public DateTime ReservationDate { get; init; }

    /// <summary>
    /// The name of the shift to get the floorplan for
    /// </summary>
    public string ShiftName { get; init; }

    public GetFloorplanByDateAndShiftQuery(Guid restaurantGuid, DateTime reservationDate, string shiftName)
    {
        RestaurantGuid = restaurantGuid;
        ReservationDate = reservationDate;
        ShiftName = shiftName;
    }
} 