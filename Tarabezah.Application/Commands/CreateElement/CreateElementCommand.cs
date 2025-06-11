using MediatR;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Application.Commands.CreateElement;

public record CreateElementCommand(
    string Name, 
    string? ImageUrl, 
    string TableType, 
    string Purpose) : IRequest<Guid>; 