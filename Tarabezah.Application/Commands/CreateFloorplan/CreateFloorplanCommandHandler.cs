using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;
using System.Linq;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Commands.CreateFloorplan;

public class CreateFloorplanCommandHandler : IRequestHandler<CreateFloorplanCommand, FloorplanDto>
{
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IRepository<Element> _elementRepository;
    private readonly ILogger<CreateFloorplanCommandHandler> _logger;

    public CreateFloorplanCommandHandler(
        IFloorplanRepository floorplanRepository,
        IRestaurantRepository restaurantRepository,
        IRepository<Element> elementRepository,
        ILogger<CreateFloorplanCommandHandler> logger)
    {
        _floorplanRepository = floorplanRepository;
        _restaurantRepository = restaurantRepository;
        _elementRepository = elementRepository;
        _logger = logger;
    }

    public async Task<FloorplanDto> Handle(CreateFloorplanCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new floorplan with name {Name} for restaurant with GUID {RestaurantGuid}",
            request.Name, request.RestaurantGuid);

        // Get restaurant by GUID first
        var restaurant = await _restaurantRepository.GetByGuidAsync(request.RestaurantGuid);

        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant with GUID {RestaurantGuid} not found", request.RestaurantGuid);
            throw new ArgumentException($"Restaurant with GUID {request.RestaurantGuid} not found");
        }

        var floorplan = new Floorplan
        {
            Name = request.Name,
            RestaurantId = restaurant.Id // Use internal ID for DB relationships
        };

        // If elements were provided, add them to the floorplan
        if (request.Elements != null && request.Elements.Any())
        {
            _logger.LogInformation("Adding {Count} elements to floorplan", request.Elements.Count);

            foreach (var elementDto in request.Elements)
            {
                // Get element by GUID
                var element = await _elementRepository.GetByGuidAsync(elementDto.ElementGuid);
                if (element == null)
                {
                    _logger.LogWarning("Element with GUID {ElementGuid} not found", elementDto.ElementGuid);
                    throw new ArgumentException($"Element with GUID {elementDto.ElementGuid} not found");
                }

                // Create and add FloorplanElementInstance
                var floorplanElement = new FloorplanElementInstance
                {
                    TableId = elementDto.TableId,
                    ElementId = element.Id, // Use internal ID for DB relationships
                    Element = element, // Include the element for mapping later
                    MinCapacity = elementDto.MinCapacity,
                    MaxCapacity = elementDto.MaxCapacity,
                    X = elementDto.X,
                    Y = elementDto.Y,
                    Height = elementDto.Height,
                    Width = elementDto.Width,
                    Rotation = elementDto.Rotation
                };

                floorplan.Elements.Add(floorplanElement);
            }
        }

        // Add the floorplan and save changes
        await _floorplanRepository.AddAsync(floorplan);
        await _floorplanRepository.SaveChangesAsync(cancellationToken);

        // Ensure elements are loaded for mapping
        await _floorplanRepository.EnsureElementsLoadedAsync(floorplan, cancellationToken);

        // Map to FloorplanDto
        return new FloorplanDto
        {
            Guid = floorplan.Guid,
            Name = floorplan.Name,
            CreatedDate = floorplan.CreatedDate,
            ModifiedDate = floorplan.ModifiedDate,
            RestaurantGuid = restaurant.Guid,
            RestaurantName = restaurant.Name,
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
                Height = e.Height,
                Width = e.Width,    
                Rotation = e.Rotation,
                CreatedDate = e.CreatedDate
            }).ToList()
        };
    }
}