using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tarabezah.Application.Commands.CreateRestaurant;
using Tarabezah.Application.Commands.CreateAndPublishFloorplans;
using Tarabezah.Application.Common;
using Tarabezah.Application.Dtos;
using Tarabezah.Application.Dtos.Notifications;
using Tarabezah.Application.Queries.GetAllRestaurants;
using Tarabezah.Application.Queries.GetRestaurantById;
using Tarabezah.Application.Queries.GetRestaurantFloorplans;
using Tarabezah.Application.Queries.GetRestaurantFloorplansWithElements;
using Tarabezah.Application.Services;

namespace Tarabezah.Web.Controllers;

/// <summary>
/// Manages restaurant operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RestaurantsController> _logger;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Creates a new instance of the RestaurantsController
    /// </summary>
    public RestaurantsController(
        IMediator mediator,
        ILogger<RestaurantsController> logger,
        INotificationService notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <summary>
    /// Gets all restaurants
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RestaurantResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var restaurants = await _mediator.Send(new GetAllRestaurantsQuery());
        return Ok(ApiResponse<IEnumerable<RestaurantResponseDto>>.Success(restaurants, "Restaurants retrieved successfully"));
    }

    /// <summary>
    /// Gets a restaurant by its GUID
    /// </summary>
    [HttpGet("{guid}")]
    [ProducesResponseType(typeof(ApiResponse<RestaurantDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RestaurantDetailResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGuid(Guid guid)
    {
        var restaurant = await _mediator.Send(new GetRestaurantByIdQuery(guid));

        if (restaurant == null)
        {
            return NotFound(ApiResponse<RestaurantDetailResponseDto>.NotFound($"Restaurant with ID {guid} not found"));
        }

        return Ok(ApiResponse<RestaurantDetailResponseDto>.Success(restaurant, "Restaurant retrieved successfully"));
    }

    /// <summary>
    /// Gets all floorplans for a restaurant
    /// </summary>
    [HttpGet("{guid}/floorplans")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FloorplanSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FloorplanSummaryDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFloorplans(Guid guid)
    {
        var floorplans = await _mediator.Send(new GetRestaurantFloorplansQuery(guid));

        if (floorplans == null)
        {
            return NotFound(ApiResponse<IEnumerable<FloorplanSummaryDto>>.NotFound($"Restaurant with ID {guid} not found"));
        }

        return Ok(ApiResponse<IEnumerable<FloorplanSummaryDto>>.Success(floorplans, "Floorplans retrieved successfully"));
    }

    /// <summary>
    /// Gets floorplans with their elements for a restaurant
    /// </summary>
    [HttpGet("{guid}/floorplans/with-elements")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FloorplanWithElementsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FloorplanWithElementsDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFloorplansWithElements(Guid guid)
    {
        var floorplans = await _mediator.Send(new GetRestaurantFloorplansWithElementsQuery(guid));

        if (floorplans == null)
        {
            return NotFound(ApiResponse<IEnumerable<FloorplanWithElementsDto>>.NotFound($"Restaurant with ID {guid} not found"));
        }

        return Ok(ApiResponse<IEnumerable<FloorplanWithElementsDto>>.Success(floorplans, "Floorplans with elements retrieved successfully"));
    }

    /// <summary>
    /// Creates or updates floorplans for a restaurant
    /// </summary>
    [HttpPost("{guid}/create-floorplans")]
    [ProducesResponseType(typeof(ApiResponse<CreateFloorplansResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CreateFloorplansResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CreateFloorplansResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFloorplans(Guid guid, [FromBody] List<CreateFloorplanDto> floorplans)
    {
        try
        {
            var command = new CreateFloorplansCommand(guid, floorplans);
            var result = await _mediator.Send(command);

            if (result.HasErrors)
            {
                if (result.ErrorMessages.Any(e => e.Contains("not found")))
                {
                    return NotFound(ApiResponse<CreateFloorplansResult>.NotFound(
                        "Restaurant or related resources not found",
                        result.ErrorMessages.FirstOrDefault()));
                }

                return BadRequest(ApiResponse<CreateFloorplansResult>.BadRequest(
                    "Failed to process floorplans",
                    "One or more errors occurred while processing floorplans",
                    result.ErrorMessages));
            }

            var successMessage = $"Successfully processed floorplans: {result.SuccessCount} created/updated, {result.DeletedCount} deleted";
            var response = ApiResponse<CreateFloorplansResult>.Created(result, successMessage);

            // Notify clients that floorplans have been published
            await _notificationService.NotifyFloorplanCreatedAsync();

            return CreatedAtAction(nameof(GetFloorplans), new { guid }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing floorplans for restaurant with GUID {RestaurantGuid}", guid);
            return BadRequest(ApiResponse<CreateFloorplansResult>.BadRequest(
                "Failed to process floorplans",
                ex.Message));
        }
    }

    /// <summary>
    /// Creates a new restaurant
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateRestaurantCommand command)
    {
        try
        {
            var guid = await _mediator.Send(command);

            // Notify clients that a restaurant has been created
            await _notificationService.NotifyRestaurantCreatedAsync();

            var response = ApiResponse<Guid>.Created(guid, "Restaurant created successfully");
            return CreatedAtAction(nameof(GetByGuid), new { guid }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<Guid>.BadRequest("Failed to create restaurant", ex.Message));
        }
    }
}
