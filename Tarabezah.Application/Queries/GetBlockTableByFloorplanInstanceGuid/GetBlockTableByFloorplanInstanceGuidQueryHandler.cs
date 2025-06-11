using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetBlockTableByFloorplanInstanceGuid;

/// <summary>
/// Handler for getting block table information by floorplan instance GUID
/// </summary>
public class GetBlockTableByFloorplanInstanceGuidQueryHandler
    : IRequestHandler<GetBlockTableByFloorplanInstanceGuidQuery, BlockTableDetailsDto>
{
    private readonly IRepository<FloorplanElementInstance> _floorplanElementRepository;
    private readonly IRepository<BlockTable> _blockedTableRepository;
    private readonly ILogger<GetBlockTableByFloorplanInstanceGuidQueryHandler> _logger;

    public GetBlockTableByFloorplanInstanceGuidQueryHandler(
        IRepository<FloorplanElementInstance> floorplanElementRepository,
        IRepository<BlockTable> blockedTableRepository,
        ILogger<GetBlockTableByFloorplanInstanceGuidQueryHandler> logger)
    {
        _floorplanElementRepository = floorplanElementRepository;
        _blockedTableRepository = blockedTableRepository;
        _logger = logger;
    }

    public async Task<BlockTableDetailsDto> Handle(
        GetBlockTableByFloorplanInstanceGuidQuery request,
        CancellationToken cancellationToken)
    {
        if (request.FloorplanElementInstanceGuid == Guid.Empty)
        {
            throw new ArgumentException("FloorplanElementInstanceGuid cannot be empty", nameof(request.FloorplanElementInstanceGuid));
        }

        _logger.LogInformation(
            "Getting block table information for floorplan element {ElementGuid}",
            request.FloorplanElementInstanceGuid);

        // Get the floorplan element instance
        var floorplanElement = await _floorplanElementRepository.GetByGuidAsync(
            request.FloorplanElementInstanceGuid);

        if (floorplanElement == null)
        {
            _logger.LogWarning(
                "Floorplan element with GUID {ElementGuid} not found",
                request.FloorplanElementInstanceGuid);
            throw new ArgumentException(
                $"Floorplan element with GUID {request.FloorplanElementInstanceGuid} not found");
        }

        // Get the latest active block for this table
        var today = DateTime.Today;
        var blockedTables = await _blockedTableRepository.GetAllWithIncludesAsync(
            includes: Array.Empty<string>());

        var latestBlock = blockedTables
            .OrderByDescending(bt => bt.StartDate)
            .ThenByDescending(bt => bt.StartTime)
            .FirstOrDefault();

        if (latestBlock == null)
        {
            _logger.LogInformation(
                "No active block found for floorplan element {ElementGuid}",
                request.FloorplanElementInstanceGuid);
            throw new ArgumentException(
                $"No active block found for floorplan element {request.FloorplanElementInstanceGuid}");
        }

        return new BlockTableDetailsDto
        {
            FloorplanElementInstanceGuid = floorplanElement.Guid,
            TableId = floorplanElement.TableId ?? string.Empty,
            StartTime = latestBlock.StartTime,
            EndTime = latestBlock.EndTime,
            StartDate = latestBlock.StartDate,
            EndDate = latestBlock.EndDate,
            Notes = latestBlock.Notes
        };
    }
}