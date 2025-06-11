using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Repositories;
using System.Linq;

namespace Tarabezah.Application.Queries.GetFloorplanById;

public class GetFloorplanByIdQueryHandler : IRequestHandler<GetFloorplanByIdQuery, FloorplanDto?>
{
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly ILogger<GetFloorplanByIdQueryHandler> _logger;

    public GetFloorplanByIdQueryHandler(
        IFloorplanRepository floorplanRepository,
        ILogger<GetFloorplanByIdQueryHandler> logger)
    {
        _floorplanRepository = floorplanRepository;
        _logger = logger;
    }

    public async Task<FloorplanDto?> Handle(GetFloorplanByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting floorplan with GUID {Guid}", request.FloorplanGuid);

        var floorplan = await _floorplanRepository.GetFloorplanWithElementsByGuidAsync(request.FloorplanGuid);

        if (floorplan == null)
        {
            _logger.LogWarning("Floorplan with GUID {Guid} not found", request.FloorplanGuid);
            return null;
        }

        var floorplanDto = new FloorplanDto
        {
            Guid = floorplan.Guid,
            Name = floorplan.Name,
            CreatedDate = floorplan.CreatedDate,
            ModifiedDate = floorplan.ModifiedDate,
            RestaurantGuid = floorplan.Restaurant?.Guid ?? Guid.Empty,
            RestaurantName = floorplan.Restaurant?.Name ?? string.Empty,
            Elements = floorplan.Elements.Select(e => new FloorplanElementResponseDto
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
            }).ToList()
        };

        return floorplanDto;
    }
}