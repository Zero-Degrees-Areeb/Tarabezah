using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetFloorplanElementById;

/// <summary>
/// Handler for retrieving a specific element in a floorplan
/// </summary>
public class GetFloorplanElementByIdQueryHandler : IRequestHandler<GetFloorplanElementByIdQuery, FloorplanElementDetailResponseDto?>
{
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly ILogger<GetFloorplanElementByIdQueryHandler> _logger;

    public GetFloorplanElementByIdQueryHandler(
        IFloorplanRepository floorplanRepository,
        ILogger<GetFloorplanElementByIdQueryHandler> logger)
    {
        _floorplanRepository = floorplanRepository;
        _logger = logger;
    }

    public async Task<FloorplanElementDetailResponseDto?> Handle(GetFloorplanElementByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving element with GUID {ElementGuid} from floorplan with GUID {FloorplanGuid}", 
            request.ElementGuid, request.FloorplanGuid);
        
        var floorplan = await _floorplanRepository.GetFloorplanWithElementsByGuidAsync(request.FloorplanGuid);
        
        if (floorplan == null)
        {
            _logger.LogWarning("Floorplan with GUID {FloorplanGuid} not found", request.FloorplanGuid);
            return null;
        }
        
        var element = floorplan.Elements.FirstOrDefault(e => e.Guid == request.ElementGuid);
        
        if (element == null)
        {
            _logger.LogWarning("Element with GUID {ElementGuid} not found in floorplan {FloorplanGuid}", 
                request.ElementGuid, request.FloorplanGuid);
            return null;
        }
        
        var elementDto = new FloorplanElementDetailResponseDto
        {
            Guid = element.Guid,
            TableId = element.TableId,
            ElementGuid = element.Element.Guid,
            ElementName = element.Element.Name,
            ElementImageUrl = element.Element.ImageUrl,
            ElementType = element.Element.TableType.ToString(),
            MinCapacity = element.MinCapacity,
            MaxCapacity = element.MaxCapacity,
            X = element.X,
            Y = element.Y,
            Height = element.Height,
            Width = element.Width,
            Rotation = element.Rotation,
            CreatedDate = element.CreatedDate,
            ModifiedDate = element.ModifiedDate
        };
        
        _logger.LogInformation("Retrieved element {TableId} from floorplan {FloorplanName}", 
            element.TableId, floorplan.Name);
        
        return elementDto;
    }
} 