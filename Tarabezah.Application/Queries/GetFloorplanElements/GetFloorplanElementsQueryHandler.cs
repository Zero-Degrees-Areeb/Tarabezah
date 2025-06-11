using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetFloorplanElements;

/// <summary>
/// Handler for retrieving all elements for a floorplan
/// </summary>
public class GetFloorplanElementsQueryHandler : IRequestHandler<GetFloorplanElementsQuery, IEnumerable<FloorplanElementResponseDto>?>
{
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly ILogger<GetFloorplanElementsQueryHandler> _logger;

    public GetFloorplanElementsQueryHandler(
        IFloorplanRepository floorplanRepository,
        ILogger<GetFloorplanElementsQueryHandler> logger)
    {
        _floorplanRepository = floorplanRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<FloorplanElementResponseDto>?> Handle(GetFloorplanElementsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving elements for floorplan with GUID {FloorplanGuid}", request.FloorplanGuid);

        var floorplan = await _floorplanRepository.GetFloorplanWithElementsByGuidAsync(request.FloorplanGuid);

        if (floorplan == null)
        {
            _logger.LogWarning("Floorplan with GUID {FloorplanGuid} not found", request.FloorplanGuid);
            return null;
        }

        var elementDtos = floorplan.Elements.Select(e => new FloorplanElementResponseDto
        {
            Guid = e.Guid,
            TableId = e.TableId,
            ElementGuid = e.Element.Guid,
            ElementName = e.Element.Name,
            ElementImageUrl = e.Element.ImageUrl,
            ElementType = e.Element.TableType.ToString(),
            MinCapacity = e.MinCapacity,
            MaxCapacity = e.MaxCapacity,
            X = e.X,
            Y = e.Y,
            Width = e.Width,
            Height = e.Height,
            Rotation = e.Rotation,
            CreatedDate = e.CreatedDate
        }).ToList();

        _logger.LogInformation("Retrieved {Count} elements for floorplan {FloorplanName}",
            elementDtos.Count, floorplan.Name);

        return elementDtos;
    }
}