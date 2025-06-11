using System.Collections.Generic;
using MediatR;

namespace Tarabezah.Application.Queries.GetEnumValues;

/// <summary>
/// Query to retrieve all available ElementPurpose enum values
/// </summary>
public record GetElementPurposesQuery : IRequest<IEnumerable<string>>; 