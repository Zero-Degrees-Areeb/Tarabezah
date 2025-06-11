using System;
using System.Collections.Generic;
using MediatR;
using Tarabezah.Application.Common;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Application.Queries.GetReservationsByDateAndShift;

/// <summary>
/// Query to get reservations and counts grouped by status for a specific restaurant, date and shift
/// </summary>
public record GetReservationsByDateAndShiftQuery : IRequest<PaginatedResponseDto<ReservationGroupsResponseDto>>
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
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; } = 10;

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
    /// Filter by reservation statuses
    /// </summary>
    public List<ReservationStatus>? Statuses { get; init; }

    /// <summary>
    /// Sort by field
    /// </summary>
    public string? SortBy { get; init; }

    public GetReservationsByDateAndShiftQuery(
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
        List<ReservationStatus>? statuses = null,
        string? sortBy = null)
    {
        RestaurantGuid = restaurantGuid;
        ReservationDate = reservationDate;
        ShiftName = shiftName;
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 10 : pageSize;
        SearchName = searchName;
        Tags = tags;
        MinPartySize = minPartySize;
        MaxPartySize = maxPartySize;
        StartTime = startTime;
        EndTime = endTime;
        Statuses = statuses;
        SortBy = sortBy;
    }
}