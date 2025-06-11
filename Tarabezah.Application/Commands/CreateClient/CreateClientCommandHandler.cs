using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Common;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.CreateClient;

/// <summary>
/// Handler for the CreateClientCommand
/// </summary>
public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IRepository<Client> _clientRepository;
    private readonly ILogger<CreateClientCommandHandler> _logger;
    private readonly TimeZoneInfo _jordanTimeZone;

    public CreateClientCommandHandler(
        IRepository<Client> clientRepository,
        ILogger<CreateClientCommandHandler> logger,
        TimeZoneInfo jordanTimeZone)
    {
        _clientRepository = clientRepository;
        _logger = logger;
        _jordanTimeZone = jordanTimeZone;
    }

    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new client: {ClientName}", request.Name);

        // Convert tag integer values to ClientTag enum values
        var tags = request.TagValues != null
            ? EnumCollectionConverter.ToEnumList<ClientTag>(request.TagValues)
            : new List<ClientTag>();

        // Convert tags to strings for storage
        var tagStrings = GetClientTagsAsStrings(tags);

        var client = new Client
        {
            Guid = Guid.NewGuid(),
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email ?? string.Empty,
            Birthday = request.Birthday,
            Source = request.Source,
            Tags = tagStrings,
            Notes = request.Notes ?? string.Empty,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        await _clientRepository.AddAsync(client);

        _logger.LogInformation("Created new client with ID: {ClientId}", client.Id);

        // convert saved UTC to Jordan before returning
        var createdJordan = TimeZoneInfo.ConvertTimeFromUtc(client.CreatedDate, _jordanTimeZone);

        // Return DTO with both enum values and string representations
        return new ClientDto
        {
            Guid = client.Guid,
            Name = client.Name,
            PhoneNumber = client.PhoneNumber,
            Email = client.Email,
            Birthday = client.Birthday,
            Source = client.Source.ToString(),
            Tags = client.Tags,
            Notes = client.Notes,
            CreatedDate = createdJordan
        };
    }

    private List<string> GetClientTagsAsStrings(List<ClientTag> tags)
    {
        return tags.Select(t => t.ToString()).ToList();
    }

    private List<ClientTag> GetClientTagsAsEnum(List<string> tags)
    {
        return tags.Select(t => Enum.Parse<ClientTag>(t)).ToList();
    }
}