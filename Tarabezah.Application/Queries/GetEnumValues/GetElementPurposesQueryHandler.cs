using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Application.Queries.GetEnumValues;

public class GetElementPurposesQueryHandler : IRequestHandler<GetElementPurposesQuery, IEnumerable<string>>
{
    private readonly ILogger<GetElementPurposesQueryHandler> _logger;

    public GetElementPurposesQueryHandler(ILogger<GetElementPurposesQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<string>> Handle(GetElementPurposesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all ElementPurpose enum values");
        
        // Get all values from the ElementPurpose enum
        var purposes = Enum.GetNames<ElementPurpose>();
        
        _logger.LogInformation("Retrieved {Count} ElementPurpose values", purposes.Length);
        
        return Task.FromResult(purposes.AsEnumerable());
    }
} 