using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetAllElements;

/// <summary>
/// Handler for retrieving all elements
/// </summary>
public class GetAllElementsQueryHandler : IRequestHandler<GetAllElementsQuery, IEnumerable<ElementResponseDto>>
{
    private readonly IRepository<Element> _elementRepository;
    private readonly ILogger<GetAllElementsQueryHandler> _logger;

    public GetAllElementsQueryHandler(
        IRepository<Element> elementRepository,
        ILogger<GetAllElementsQueryHandler> logger)
    {
        _elementRepository = elementRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ElementResponseDto>> Handle(GetAllElementsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all elements");
        
        var elements = await _elementRepository.GetAllAsync();
        
        var elementDtos = elements.Select(e => new ElementResponseDto
        {
            Guid = e.Guid,
            Name = e.Name,
            ImageUrl = e.ImageUrl,
            TableType = e.TableType.ToString(),
            Purpose = e.Purpose.ToString(),
            CreatedDate = e.CreatedDate
        });
        
        _logger.LogInformation("Retrieved {Count} elements", elementDtos.Count());
        
        return elementDtos;
    }
} 