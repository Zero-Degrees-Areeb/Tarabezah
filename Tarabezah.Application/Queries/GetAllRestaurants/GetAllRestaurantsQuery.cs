using System.Collections.Generic;
using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetAllRestaurants;

/// <summary>
/// Query to retrieve all restaurants
/// </summary>
public record GetAllRestaurantsQuery() : IRequest<IEnumerable<RestaurantResponseDto>>; 