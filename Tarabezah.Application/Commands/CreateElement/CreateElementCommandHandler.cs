using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.CreateElement;

public class CreateElementCommandHandler : IRequestHandler<CreateElementCommand, Guid>
{
    private readonly IRepository<Element> _elementRepository;
    private readonly ILogger<CreateElementCommandHandler> _logger;

    public CreateElementCommandHandler(
        IRepository<Element> elementRepository,
        ILogger<CreateElementCommandHandler> logger)
    {
        _elementRepository = elementRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateElementCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new element with name {Name}", request.Name);

        // Parse the string values to the corresponding enums
        if (!Enum.TryParse<TableType>(request.TableType, out var tableType))
        {
            throw new ArgumentException($"Invalid table type. Valid values are: {string.Join(", ", Enum.GetNames<TableType>())}");
        }

        if (!Enum.TryParse<ElementPurpose>(request.Purpose, out var purpose))
        {
            throw new ArgumentException($"Invalid purpose. Valid values are: {string.Join(", ", Enum.GetNames<ElementPurpose>())}");
        }

        var element = new Element
        {
            Name = request.Name,
            ImageUrl = request.ImageUrl ?? string.Empty,
            TableType = tableType,
            Purpose = purpose
        };

        var createdElement = await _elementRepository.AddAsync(element);
        
        _logger.LogInformation("Successfully created element with GUID {Guid}", createdElement.Guid);
        
        return createdElement.Guid;
    }
} 