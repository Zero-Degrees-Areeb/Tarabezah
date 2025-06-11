using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetRestaurantById;

/// <summary>
/// Query to retrieve a specific restaurant by ID
/// </summary>
public record GetRestaurantByIdQuery(Guid RestaurantGuid) : IRequest<RestaurantDetailResponseDto?>; 