using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetElementById;

/// <summary>
/// Handler for retrieving a specific element by ID
/// </summary>
public class GetElementByIdQueryHandler : IRequestHandler<GetElementByIdQuery, ElementResponseDto?>
{
    private readonly IRepository<Element> _elementRepository;
    private readonly ILogger<GetElementByIdQueryHandler> _logger;

    public GetElementByIdQueryHandler(
        IRepository<Element> elementRepository,
        ILogger<GetElementByIdQueryHandler> logger)
    {
        _elementRepository = elementRepository;
        _logger = logger;
    }

    public async Task<ElementResponseDto?> Handle(GetElementByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving element with GUID {ElementGuid}", request.ElementGuid);
        
        var element = await _elementRepository.GetByGuidAsync(request.ElementGuid);
        
        if (element == null)
        {
            _logger.LogWarning("Element with GUID {ElementGuid} not found", request.ElementGuid);
            return null;
        }
        
        var elementDto = new ElementResponseDto
        {
            Guid = element.Guid,
            Name = element.Name,
            ImageUrl = element.ImageUrl,
            TableType = element.TableType.ToString(),
            Purpose = element.Purpose.ToString(),
            CreatedDate = element.CreatedDate,
            ModifiedDate = element.ModifiedDate
        };
        
        _logger.LogInformation("Retrieved element {ElementName} with GUID {ElementGuid}", element.Name, element.Guid);
        
        return elementDto;
    }
} 