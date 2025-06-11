using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.CreateFloorplanElement;

public class CreateFloorplanElementCommandHandler : IRequestHandler<CreateFloorplanElementCommand, Guid>
{
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly IRepository<Element> _elementRepository;
    private readonly IRepository<FloorplanElementInstance> _floorplanElementRepository;
    private readonly ILogger<CreateFloorplanElementCommandHandler> _logger;

    public CreateFloorplanElementCommandHandler(
        IFloorplanRepository floorplanRepository,
        IRepository<Element> elementRepository,
        IRepository<FloorplanElementInstance> floorplanElementRepository,
        ILogger<CreateFloorplanElementCommandHandler> logger)
    {
        _floorplanRepository = floorplanRepository;
        _elementRepository = elementRepository;
        _floorplanElementRepository = floorplanElementRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateFloorplanElementCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding element with GUID {ElementGuid} to floorplan with GUID {FloorplanGuid}",
            request.ElementGuid, request.FloorplanGuid);

        // Get floorplan by GUID
        var floorplan = await _floorplanRepository.GetByGuidAsync(request.FloorplanGuid);
        if (floorplan == null)
        {
            _logger.LogWarning("Floorplan with GUID {FloorplanGuid} not found", request.FloorplanGuid);
            throw new ArgumentException($"Floorplan with GUID {request.FloorplanGuid} not found");
        }

        // Get element by GUID
        var element = await _elementRepository.GetByGuidAsync(request.ElementGuid);
        if (element == null)
        {
            _logger.LogWarning("Element with GUID {ElementGuid} not found", request.ElementGuid);
            throw new ArgumentException($"Element with GUID {request.ElementGuid} not found");
        }

        // Check if TableId is already used in this floorplan
        var existingElements = await _floorplanRepository.GetFloorplanWithElementsByGuidAsync(request.FloorplanGuid);
        if (existingElements?.Elements.Any(e => e.TableId == request.TableId) == true)
        {
            _logger.LogWarning("Table ID {TableId} is already in use on floorplan with GUID {FloorplanGuid}",
                request.TableId, request.FloorplanGuid);
            throw new ArgumentException($"Table ID '{request.TableId}' is already in use on this floorplan");
        }

        // Create the floorplan element instance
        var floorplanElement = new FloorplanElementInstance
        {
            FloorplanId = floorplan.Id, // Use internal ID for DB relationships
            ElementId = element.Id, // Use internal ID for DB relationships
            TableId = request.TableId,
            MinCapacity = request.MinCapacity,
            MaxCapacity = request.MaxCapacity,
            X = request.X,
            Y = request.Y,
            Height = request.Height,
            Width = request.Width,
            Rotation = request.Rotation
        };

        var createdElement = await _floorplanElementRepository.AddAsync(floorplanElement);

        _logger.LogInformation("Successfully added element with GUID {ElementGuid} to floorplan with GUID {FloorplanGuid}, created instance with GUID {InstanceGuid}",
            request.ElementGuid, request.FloorplanGuid, createdElement.Guid);

        return createdElement.Guid;
    }
} 