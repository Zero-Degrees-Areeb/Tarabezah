using MediatR;
using System;
using System.Collections.Generic;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetRestaurantShifts;

/// <summary>
/// Query to retrieve shifts for a specific restaurant
/// </summary>
public class GetRestaurantShiftsQuery : IRequest<IEnumerable<ShiftDto>>
{
    /// <summary>
    /// The GUID of the restaurant to get shifts for
    /// </summary>
    public Guid RestaurantGuid { get; set; }
} 