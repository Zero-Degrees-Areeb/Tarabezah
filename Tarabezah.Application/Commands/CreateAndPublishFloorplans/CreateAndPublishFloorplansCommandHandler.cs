using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.CreateAndPublishFloorplans;

public class CreateFloorplansCommandHandler : IRequestHandler<CreateFloorplansCommand, CreateFloorplansResult>
{
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IRepository<Element> _elementRepository;
    private readonly IRepository<FloorplanElementInstance> _floorplanElementInstanceRepository;
    private readonly IRepository<CombinedTableMember> _combinedTableMemberRepository;
    private readonly ILogger<CreateFloorplansCommandHandler> _logger;

    public CreateFloorplansCommandHandler(
        IFloorplanRepository floorplanRepository,
        IRestaurantRepository restaurantRepository,
        IRepository<Element> elementRepository,
        IRepository<FloorplanElementInstance> floorplanElementInstanceRepository,
        IRepository<CombinedTableMember> combinedTableMemberRepository,
        ILogger<CreateFloorplansCommandHandler> logger)
    {
        _floorplanRepository = floorplanRepository;
        _restaurantRepository = restaurantRepository;
        _elementRepository = elementRepository;
        _floorplanElementInstanceRepository = floorplanElementInstanceRepository;
        _combinedTableMemberRepository = combinedTableMemberRepository;
        _logger = logger;
    }

    public async Task<CreateFloorplansResult> Handle(
        CreateFloorplansCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating or updating {FloorplanCount} floorplans for restaurant with GUID {RestaurantGuid}",
            request.Floorplans.Count, request.RestaurantGuid);

        var result = new CreateFloorplansResult
        {
            RestaurantGuid = request.RestaurantGuid,
            TotalFloorplanCount = request.Floorplans.Count
        };

        // Verify restaurant exists
        var restaurant = await _restaurantRepository.GetByGuidAsync(request.RestaurantGuid);
        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant with GUID {RestaurantGuid} not found", request.RestaurantGuid);
            result.ErrorMessages.Add($"Restaurant with GUID {request.RestaurantGuid} not found");
            return result;
        }

        // Get existing floorplans for this restaurant to handle deletion of missing ones
        var existingFloorplans = await _floorplanRepository.GetFloorplansByRestaurantGuidAsync(request.RestaurantGuid);
        var existingFloorplansByName = existingFloorplans.ToDictionary(f => f.Guid, f => f);

        // Keep track of processed floorplan names to identify which ones to delete later
        var processedFloorplans = new HashSet<Guid>();

        // Process each floorplan
        foreach (var floorplanDto in request.Floorplans)
        {
            try
            {
                processedFloorplans.Add(floorplanDto.Guid);

                // Check if floorplan exists by name
                Floorplan floorplan;
                bool isNewFloorplan = false;

                if (existingFloorplansByName.TryGetValue(floorplanDto.Guid, out var existingFloorplan))
                {
                    // Update existing floorplan
                    _logger.LogInformation("Updating existing floorplan {FloorplanName} with GUID {FloorplanGuid}",
                        existingFloorplan.Name, existingFloorplan.Guid);

                    floorplan = existingFloorplan;
                    floorplan.Name = floorplanDto.Name;
                }
                else
                {
                    // Create a new floorplan
                    _logger.LogInformation("Creating new floorplan {FloorplanName} for restaurant {RestaurantGuid}",
                        floorplanDto.Name, request.RestaurantGuid);

                    floorplan = new Floorplan
                    {
                        Name = floorplanDto.Name,
                        RestaurantId = restaurant.Id // Use internal ID for DB relationships
                    };

                    isNewFloorplan = true;
                }

                // Process elements if any
                var existingElements = floorplan.Elements.ToList();

                if (floorplanDto.Elements != null && !floorplanDto.Elements.Any())
                {
                    // If the Elements collection is explicitly empty, delete all existing elements
                    _logger.LogInformation("Elements collection is empty for floorplan {FloorplanName}. Deleting all existing elements.",
                        floorplan.Name);

                    foreach (var elementToDelete in existingElements)
                    {
                        // Use the updated DeleteDirectAsync method to handle constraints
                        await _floorplanElementInstanceRepository.DeleteDirectAsync(elementToDelete);

                        _logger.LogInformation("Successfully deleted element {TableId} with GUID {ElementGuid}.",
                            elementToDelete.TableId, elementToDelete.Guid);
                    }
                }
                else if (floorplanDto.Elements != null && floorplanDto.Elements.Any())
                {
                    _logger.LogInformation("Processing {ElementCount} elements for floorplan {FloorplanName}",
                        floorplanDto.Elements.Count, floorplanDto.Name);

                    // Create dictionaries for both GUID and TableId lookups
                    var existingElementsByGuid = floorplan.Elements.ToDictionary(e => e.Guid);
                    var processedElementGuids = new HashSet<Guid>();
                    var processedTableIds = new HashSet<string>();

                    foreach (var elementDto in floorplanDto.Elements)
                    {
                        try
                        {
                            // Get element by GUID
                            var element = await _elementRepository.GetByGuidAsync(elementDto.ElementGuid);
                            if (element == null)
                            {
                                throw new ArgumentException($"Element with GUID {elementDto.ElementGuid} not found");
                            }

                            // First check if we have an existing element with the same TableId
                            FloorplanElementInstance existingElement = null;
                            if (existingElementsByGuid.TryGetValue(elementDto.Guid, out existingElement))
                            {
                                _logger.LogInformation("Updating existing element {ElementGuid} in floorplan {FloorplanName}",
                                    elementDto.Guid, floorplan.Name);
                            }

                            if (existingElement != null)
                            {
                                // Update existing element
                                existingElement.TableId = elementDto.TableId;
                                existingElement.ElementId = element.Id;
                                existingElement.Element = element;
                                existingElement.MinCapacity = elementDto.MinCapacity;
                                existingElement.MaxCapacity = elementDto.MaxCapacity;
                                existingElement.X = elementDto.X;
                                existingElement.Y = elementDto.Y;
                                existingElement.Height = elementDto.Height;
                                existingElement.Width = elementDto.Width;
                                existingElement.Rotation = elementDto.Rotation;
                            }
                            else
                            {
                                // Before creating new element, check TableId uniqueness only for Reservable elements
                                if (!string.IsNullOrEmpty(elementDto.TableId) &&
                                    element.Purpose == ElementPurpose.Reservable &&
                                    processedTableIds.Contains(elementDto.TableId))
                                {
                                    throw new InvalidOperationException($"Duplicate TableId '{elementDto.TableId}' found in the request for a reservable element");
                                }

                                // Create and add new FloorplanElementInstance
                                _logger.LogInformation("Adding new element {ElementGuid} to floorplan {FloorplanName}",
                                    elementDto.ElementGuid, floorplan.Name);

                                var floorplanElement = new FloorplanElementInstance
                                {
                                    TableId = elementDto.TableId,
                                    ElementId = element.Id,
                                    Element = element,
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

                            processedElementGuids.Add(elementDto.Guid);
                            // Only track TableIds for Reservable elements
                            if (!string.IsNullOrEmpty(elementDto.TableId) && element.Purpose == ElementPurpose.Reservable)
                            {
                                processedTableIds.Add(elementDto.TableId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing element {ElementGuid} for floorplan {FloorplanName}",
                                elementDto.Guid, floorplan.Name);
                            result.ErrorMessages.Add($"Error processing element in floorplan '{floorplan.Name}': {ex.Message}");
                        }
                    }

                    try
                    {
                        var elementsToDelete = existingElements.Where(f => !processedElementGuids.Contains(f.Guid)).ToList();

                        foreach (var elementToDelete in elementsToDelete)
                        {
                            _logger.LogInformation("Deleting element {TableId} with GUID {ElementGuid} as it was not in the request",
                                elementToDelete.TableId, elementToDelete.Guid);

                            // First delete any CombinedTableMemberships
                            foreach (var membership in elementToDelete.CombinedTableMemberships.ToList())
                            {
                                await _combinedTableMemberRepository.DeleteAsync(membership);
                            }

                            // Then delete the element
                            await _floorplanElementInstanceRepository.DeleteAsync(elementToDelete);
                        }

                        // Save changes after processing all elements
                        if (elementsToDelete.Any())
                        {
                            await _floorplanRepository.SaveChangesAsync(cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting elements that were not in the request");
                        result.ErrorMessages.Add($"Error deleting some elements: {ex.Message}");
                    }
                }

                // Save the floorplan
                Floorplan savedFloorplan;
                if (isNewFloorplan)
                {
                    await _floorplanRepository.AddAsync(floorplan);
                    savedFloorplan = floorplan;
                }
                else
                {
                    await _floorplanRepository.UpdateAsync(floorplan);
                    savedFloorplan = floorplan;
                }

                // Save changes to ensure the floorplan is persisted
                await _floorplanRepository.SaveChangesAsync(cancellationToken);

                // Ensure elements are loaded for mapping
                await _floorplanRepository.EnsureElementsLoadedAsync(savedFloorplan, cancellationToken);

                // Map to result DTO
                var mappedElements = savedFloorplan.Elements.Select(e => new FloorplanElementResponseDto
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

                // Add to result
                result.CreatedFloorplans.Add(new CreatedFloorplanDto
                {
                    Guid = savedFloorplan.Guid,
                    Name = savedFloorplan.Name,
                    CreatedDate = savedFloorplan.CreatedDate,
                    Elements = mappedElements
                });

                result.SuccessCount++;

                _logger.LogInformation("Successfully {Action} floorplan with GUID {FloorplanGuid}",
                    isNewFloorplan ? "created" : "updated", savedFloorplan.Guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing floorplan {FloorplanName}",
                    floorplanDto.Name);
                result.ErrorMessages.Add($"Error processing floorplan '{floorplanDto.Name}': {ex.Message}");
            }
        }

        // Delete floorplans that were not in the request
        try
        {
            var floorplansToDelete = existingFloorplans.Where(f => !processedFloorplans.Contains(f.Guid)).ToList();

            foreach (var floorplanToDelete in floorplansToDelete)
            {
                _logger.LogInformation("Deleting floorplan {FloorplanName} with GUID {FloorplanGuid} as it was not in the request",
                    floorplanToDelete.Name, floorplanToDelete.Guid);

                // First, get all FloorplanElementInstances for this floorplan
                var elements = floorplanToDelete.Elements.ToList();

                foreach (var element in elements)
                {
                    // Delete any CombinedTableMemberships first (these have Restrict behavior)
                    foreach (var membership in element.CombinedTableMemberships.ToList())
                    {
                        _logger.LogInformation("Deleting CombinedTableMembership with ID {MembershipId} for FloorplanElementInstance {ElementId}",
                            membership.Id, element.Id);

                        await _combinedTableMemberRepository.DeleteAsync(membership);
                    }

                    // Now delete the FloorplanElementInstance
                    _logger.LogInformation("Deleting FloorplanElementInstance with ID {ElementId}", element.Id);
                    await _floorplanElementInstanceRepository.DeleteDirectAsync(element);
                }

                // Now we can safely delete the floorplan
                _logger.LogInformation("Deleting Floorplan with GUID {FloorplanGuid}", floorplanToDelete.Guid);
                await _floorplanRepository.DeleteAsync(floorplanToDelete);
                result.DeletedCount++;

            }

            // Save all changes
            await _floorplanRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting floorplans that were not in the request");
            result.ErrorMessages.Add($"Error deleting some floorplans: {ex.Message}");
        }

        _logger.LogInformation("Processed floorplans for restaurant with GUID {RestaurantGuid}: {SuccessCount} created/updated, {DeletedCount} deleted",
            request.RestaurantGuid, result.SuccessCount, result.DeletedCount);

        return result;
    }
}