using System.Collections.Generic;
using MediatR;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetAllElements;

/// <summary>
/// Query to retrieve all elements
/// </summary>
public record GetAllElementsQuery() : IRequest<IEnumerable<ElementResponseDto>>; 