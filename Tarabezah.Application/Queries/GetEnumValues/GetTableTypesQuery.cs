using System.Collections.Generic;
using MediatR;

namespace Tarabezah.Application.Queries.GetEnumValues;

/// <summary>
/// Query to retrieve all available TableType enum values
/// </summary>
public record GetTableTypesQuery : IRequest<IEnumerable<string>>; 