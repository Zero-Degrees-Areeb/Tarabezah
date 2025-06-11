using MediatR;
using System;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetEnumValues;

/// <summary>
/// Query to retrieve lookup data including enums and restaurant shifts
/// </summary>
public class GetLookupDataQuery : IRequest<LookupDataDto>
{
    /// <summary>
    /// The GUID of the restaurant to get shifts for
    /// </summary>
    public Guid RestaurantGuid { get; set; }
} 