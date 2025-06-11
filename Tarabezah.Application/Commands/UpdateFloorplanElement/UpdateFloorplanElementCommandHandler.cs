using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.UpdateFloorplanElement;

public class UpdateFloorplanElementCommandHandler : IRequestHandler<UpdateFloorplanElementCommand, Guid>
{
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly IRepository<FloorplanElementInstance> _floorplanElementRepository;
    private readonly ILogger<UpdateFloorplanElementCommandHandler> _logger;

    public UpdateFloorplanElementCommandHandler(
        IFloorplanRepository floorplanRepository,
        IRepository<FloorplanElementInstance> floorplanElementRepository,
        ILogger<UpdateFloorplanElementCommandHandler> logger)
    {
        _floorplanRepository = floorplanRepository;
        _floorplanElementRepository = floorplanElementRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(UpdateFloorplanElementCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating element with GUID {ElementInstanceGuid} on floorplan with GUID {FloorplanGuid}",
            request.ElementInstanceGuid, request.FloorplanGuid);

        // Get floorplan by GUID
        var floorplan = await _floorplanRepository.GetByGuidAsync(request.FloorplanGuid);
        if (floorplan == null)
        {
            _logger.LogWarning("Floorplan with GUID {FloorplanGuid} not found", request.FloorplanGuid);
            throw new ArgumentException($"Floorplan with GUID {request.FloorplanGuid} not found");
        }

        // Get floorplan element by GUID
        var floorplanElement = await _floorplanElementRepository.GetByGuidAsync(request.ElementInstanceGuid);
        if (floorplanElement == null)
        {
            _logger.LogWarning("Floorplan element with GUID {ElementInstanceGuid} not found", request.ElementInstanceGuid);
            throw new ArgumentException($"Element with GUID {request.ElementInstanceGuid} not found");
        }

        // Verify element belongs to the specified floorplan
        if (floorplanElement.FloorplanId != floorplan.Id)
        {
            _logger.LogWarning("Element with GUID {ElementInstanceGuid} does not belong to floorplan with GUID {FloorplanGuid}",
                request.ElementInstanceGuid, request.FloorplanGuid);
            throw new ArgumentException($"Element does not belong to the specified floorplan");
        }

        // Check if TableId is already used by another element in this floorplan
        if (request.TableId != floorplanElement.TableId)
        {
            var existingElements = await _floorplanRepository.GetFloorplanWithElementsAsync(floorplan.Id);
            if (existingElements?.Elements.Any(e => e.TableId == request.TableId && e.Id != floorplanElement.Id) == true)
            {
                _logger.LogWarning("Table ID {TableId} is already in use on floorplan with GUID {FloorplanGuid}",
                    request.TableId, request.FloorplanGuid);
                throw new ArgumentException($"Table ID '{request.TableId}' is already in use on this floorplan");
            }

            floorplanElement.TableId = request.TableId;
        }

        // Update the element properties
        floorplanElement.MinCapacity = request.MinCapacity;
        floorplanElement.MaxCapacity = request.MaxCapacity;
        floorplanElement.X = request.X;
        floorplanElement.Y = request.Y;
        floorplanElement.Width = request.Width;
        floorplanElement.Height = request.Height;
        floorplanElement.Rotation = request.Rotation;

        await _floorplanElementRepository.UpdateAsync(floorplanElement);
        
        _logger.LogInformation("Successfully updated element on floorplan. Element instance GUID: {Guid}", floorplanElement.Guid);
        
        return floorplanElement.Guid;
    }
} 