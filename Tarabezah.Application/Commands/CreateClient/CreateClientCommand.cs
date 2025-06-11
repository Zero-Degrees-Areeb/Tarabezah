using MediatR;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Application.Commands.CreateClient;

/// <summary>
/// Command to create a new client
/// </summary>
public class CreateClientCommand : IRequest<ClientDto>
{
    /// <summary>
    /// The name of the client
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The client's phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// The client's email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The client's birthday (optional)
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// Where the client came from (numeric value of ClientSource enum)
    /// </summary>
    public ClientSource Source { get; set; } = ClientSource.Other;

    /// <summary>
    /// Tags describing the client's preferences (numeric values of ClientTag enum)
    /// </summary>
    public List<int>? TagValues { get; set; }

    /// <summary>
    /// Internal notes about the client
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// The GUID of the restaurant this client is associated with
    /// </summary>
    public Guid RestaurantGuid { get; set; }
}