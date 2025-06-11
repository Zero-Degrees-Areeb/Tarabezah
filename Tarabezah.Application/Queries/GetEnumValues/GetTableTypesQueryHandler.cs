using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Application.Queries.GetEnumValues;

public class GetTableTypesQueryHandler : IRequestHandler<GetTableTypesQuery, IEnumerable<string>>
{
    private readonly ILogger<GetTableTypesQueryHandler> _logger;

    public GetTableTypesQueryHandler(ILogger<GetTableTypesQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<string>> Handle(GetTableTypesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all TableType enum values");
        
        // Get all values from the TableType enum
        var tableTypes = Enum.GetNames<TableType>();
        
        _logger.LogInformation("Retrieved {Count} TableType values", tableTypes.Length);
        
        return Task.FromResult(tableTypes.AsEnumerable());
    }
} 