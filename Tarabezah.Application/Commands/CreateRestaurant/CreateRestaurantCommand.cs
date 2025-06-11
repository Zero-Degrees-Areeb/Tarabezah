using MediatR;

namespace Tarabezah.Application.Commands.CreateRestaurant;

public record CreateRestaurantCommand(string Name) : IRequest<Guid>;