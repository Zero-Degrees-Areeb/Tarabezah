using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;

namespace Tarabezah.Application.Queries.GetReservationStatus;

/// <summary>
/// Handler for getting all available reservation statuses
/// </summary>
public class GetReservationStatusQueryHandler : IRequestHandler<GetReservationStatusQuery, List<ReservationStatusDto>>
{
    private readonly ILogger<GetReservationStatusQueryHandler> _logger;

    public GetReservationStatusQueryHandler(ILogger<GetReservationStatusQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<List<ReservationStatusDto>> Handle(GetReservationStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all reservation statuses");

        // Get all values from the ReservationStatus enum
        var statuses = Enum.GetValues(typeof(ReservationStatus))
            .Cast<ReservationStatus>()
            .Select(status => new ReservationStatusDto
            {
                Value = (int)status,
                Name = status.ToString()
            })
            .ToList();

        _logger.LogInformation("Retrieved {Count} reservation statuses", statuses.Count);

        return Task.FromResult(statuses);
    }
}