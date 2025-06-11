using System;
using System.Collections.Generic;
using MediatR;
using Tarabezah.Application.Dtos.Reservations;

namespace Tarabezah.Application.Queries.GetWaitlistReservationByDateAndShift;

/// <summary>
/// Query to get waitlist reservations for a specific date and shift
/// </summary>
public record GetWaitlistReservationByDateAndShiftQuery : IRequest<WaitlistReservationResponseDto>
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

    /// <summary>
    /// The page number for pagination
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// The page size for pagination
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Search term for client name or phone number
    /// </summary>
    public string? SearchName { get; init; }

    /// <summary>
    /// Filter by reservation tags
    /// </summary>
    public List<string>? Tags { get; init; }

    /// <summary>
    /// Filter by minimum party size
    /// </summary>
    public int? MinPartySize { get; init; }

    /// <summary>
    /// Filter by maximum party size
    /// </summary>
    public int? MaxPartySize { get; init; }

    /// <summary>
    /// Filter by start time (inclusive)
    /// </summary>
    public TimeSpan? StartTime { get; init; }

    /// <summary>
    /// Filter by end time (inclusive)
    /// </summary>
    public TimeSpan? EndTime { get; init; }

    /// <summary>
    /// Sort by field ("ClientName" or "Time")
    /// </summary>
    public string? SortBy { get; init; }

    public GetWaitlistReservationByDateAndShiftQuery(
        Guid restaurantGuid,
        DateTime reservationDate,
        string shiftName,
        int pageNumber = 1,
        int pageSize = 10,
        string? searchName = null,
        List<string>? tags = null,
        int? minPartySize = null,
        int? maxPartySize = null,
        TimeSpan? startTime = null,
        TimeSpan? endTime = null,
        string? sortBy = null)
    {
        RestaurantGuid = restaurantGuid;
        ReservationDate = reservationDate;
        ShiftName = shiftName;
        PageNumber = pageNumber;
        PageSize = pageSize;
        SearchName = searchName;
        Tags = tags;
        MinPartySize = minPartySize;
        MaxPartySize = maxPartySize;
        StartTime = startTime;
        EndTime = endTime;
        SortBy = sortBy;
    }
}