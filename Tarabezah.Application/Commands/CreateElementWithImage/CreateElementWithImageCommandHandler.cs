using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;
using Tarabezah.Infrastructure.Services;

namespace Tarabezah.Application.Commands.CreateElementWithImage;

public class CreateElementWithImageCommandHandler : IRequestHandler<CreateElementWithImageCommand, Guid>
{
    private readonly IRepository<Element> _elementRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<CreateElementWithImageCommandHandler> _logger;

    public CreateElementWithImageCommandHandler(
        IRepository<Element> elementRepository,
        IFileUploadService fileUploadService,
        ILogger<CreateElementWithImageCommandHandler> logger)
    {
        _elementRepository = elementRepository;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateElementWithImageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new element with name {Name} and uploading image", request.Name);

        if (request.ImageFile == null || request.ImageFile.Length == 0)
        {
            throw new ArgumentException("No image file provided or file is empty");
        }

        // Parse the string values to the corresponding enums
        if (!Enum.TryParse<TableType>(request.TableType, out var tableType))
        {
            throw new ArgumentException($"Invalid table type. Valid values are: {string.Join(", ", Enum.GetNames<TableType>())}");
        }

        if (!Enum.TryParse<ElementPurpose>(request.Purpose, out var purpose))
        {
            throw new ArgumentException($"Invalid purpose. Valid values are: {string.Join(", ", Enum.GetNames<ElementPurpose>())}");
        }

        // Upload the image file and get URL
        string imageUrl;
        using (var stream = request.ImageFile.OpenReadStream())
        {
            imageUrl = await _fileUploadService.UploadFileAsync(
                stream, 
                request.ImageFile.FileName, 
                request.ImageFile.ContentType);
        }

        _logger.LogInformation("Image uploaded successfully. URL: {ImageUrl}", imageUrl);

        // Create new element with the image URL
        var element = new Element
        {
            Name = request.Name,
            ImageUrl = imageUrl,
            TableType = tableType,
            Purpose = purpose,
            Guid = Guid.NewGuid()
        };

        await _elementRepository.AddAsync(element);
        
        _logger.LogInformation("Element created successfully with ID {ElementId}", element.Guid);
        
        return element.Guid;
    }
} 