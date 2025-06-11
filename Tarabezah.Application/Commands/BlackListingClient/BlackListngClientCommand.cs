using MediatR;
using Tarabezah.Application.Dtos.Clients;

namespace Tarabezah.Application.Commands.BlackListingClient;

/// <summary>
/// Command to block or unblock a client at a restaurant
/// </summary>
public record BlackListngClientCommand(
    Guid ClientGuid,
    Guid RestaurantGuid,
    bool IsBlocked,
    string? Reason = null) : IRequest<BlackListDto>;