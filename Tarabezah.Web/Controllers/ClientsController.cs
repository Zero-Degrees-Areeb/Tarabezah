using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tarabezah.Application.Commands.CreateClient;
using Tarabezah.Application.Common;
using Tarabezah.Application.Dtos;
using Tarabezah.Application.Dtos.Notifications;
using Tarabezah.Application.Queries.SearchClients;
using Tarabezah.Application.Services;
using Microsoft.AspNetCore.SignalR;
using Tarabezah.Infrastructure.SignalR;

namespace Tarabezah.Web.Controllers;

/// <summary>
/// Controller for client operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClientsController> _logger;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the ClientsController
    /// </summary>
    public ClientsController(
        IMediator mediator,
        ILogger<ClientsController> logger,
        INotificationService notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <summary>
    /// Creates a new client
    /// </summary>
    /// <param name="command">The client information</param>
    /// <returns>The created client</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ClientDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ClientDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientCommand command)
    {
        _logger.LogInformation("Received request to create client: {ClientName}", command.Name);

        try
        {
            var result = await _mediator.Send(command);

            // Notify clients that a new client has been created
            await _notificationService.NotifyClientCreatedAsync();

            return Created($"api/clients/{result.Guid}",
                ApiResponse<ClientDto>.Created(result, "Client created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client: {ErrorMessage}", ex.Message);
            return BadRequest(ApiResponse<ClientDto>.BadRequest("Failed to create client", ex.Message));
        }
    }

    /// <summary>
    /// Searches for clients by name, phone, or email
    /// </summary>
    /// <param name="query">The search term</param>
    /// <param name="restaurantGuid">The GUID of the restaurant to use for calculating statistics</param>
    /// <returns>List of matching clients</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClientDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchClients([FromQuery] string query, [FromQuery] Guid restaurantGuid)
    {
        if (restaurantGuid == Guid.Empty)
        {
            return BadRequest(ApiResponse<string>.BadRequest("Restaurant GUID is required"));
        }

        _logger.LogInformation("Searching clients with query: {Query} for restaurant: {RestaurantGuid}",
            query, restaurantGuid);

        var result = await _mediator.Send(new SearchClientsQuery
        {
            SearchTerm = query ?? string.Empty,
            RestaurantGuid = restaurantGuid
        });

        return Ok(ApiResponse<IEnumerable<ClientDto>>.Success(result, "Clients retrieved successfully"));
    }
}