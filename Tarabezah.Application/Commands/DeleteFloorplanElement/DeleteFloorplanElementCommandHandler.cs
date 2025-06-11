using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.DeleteFloorplanElement;

/// <summary>
/// Handler for deleting a specific element in a floorplan
/// </summary>
public class DeleteFloorplanElementCommandHandler : IRequestHandler<DeleteFloorplanElementCommand, bool>
{
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly IRepository<FloorplanElementInstance> _floorplanElementRepository;
    private readonly ILogger<DeleteFloorplanElementCommandHandler> _logger;

    public DeleteFloorplanElementCommandHandler(
        IFloorplanRepository floorplanRepository,
        IRepository<FloorplanElementInstance> floorplanElementRepository,
        ILogger<DeleteFloorplanElementCommandHandler> logger)
    {
        _floorplanRepository = floorplanRepository;
        _floorplanElementRepository = floorplanElementRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteFloorplanElementCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting element with GUID {ElementGuid} from floorplan with GUID {FloorplanGuid}", 
            request.ElementGuid, request.FloorplanGuid);
        
        var floorplan = await _floorplanRepository.GetFloorplanWithElementsByGuidAsync(request.FloorplanGuid);
        
        if (floorplan == null)
        {
            _logger.LogWarning("Floorplan with GUID {FloorplanGuid} not found", request.FloorplanGuid);
            return false;
        }
        
        var element = floorplan.Elements.FirstOrDefault(e => e.Guid == request.ElementGuid);
        
        if (element == null)
        {
            _logger.LogWarning("Element with GUID {ElementGuid} not found in floorplan {FloorplanGuid}", 
                request.ElementGuid, request.FloorplanGuid);
            return false;
        }
        
        await _floorplanElementRepository.DeleteAsync(element);
        
        _logger.LogInformation("Successfully deleted element {TableId} from floorplan {FloorplanName}", 
            element.TableId, floorplan.Name);
        
        return true;
    }
} 