using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetElementById;

/// <summary>
/// Query to retrieve a specific element by ID
/// </summary>
public record GetElementByIdQuery(Guid ElementGuid) : IRequest<ElementResponseDto?>; 