using MediatR;
using System;
using System.Collections.Generic;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.SearchClients;

/// <summary>
/// Query to search for clients by name, phone number, or email
/// </summary>
public class SearchClientsQuery : IRequest<IEnumerable<ClientDto>>
{
    /// <summary>
    /// The search term to use (will be matched against name, phone, or email)
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;
    
    /// <summary>
    /// The GUID of the restaurant to use for calculating statistics
    /// </summary>
    public Guid RestaurantGuid { get; set; }
} 