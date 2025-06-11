using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Tarabezah.Application.Commands.CreateFloorplan;
using Tarabezah.Application.Commands.CreateCombinedTable;
using Tarabezah.Application.Common;
using Tarabezah.Application.Dtos;
using Tarabezah.Application.Queries.GetFloorplanById;
using Tarabezah.Application.Queries.GetCombinedTables;
using Tarabezah.Application.Queries.GetFloorplanByDateAndShift;
using Tarabezah.Application.Dtos.Floorplans;
using Tarabezah.Application.Queries.GetRestaurantById;
using Tarabezah.Application.Queries.GetRestaurantCombinedTables;
using Tarabezah.Infrastructure.SignalR;
using Tarabezah.Application.Services;
using Tarabezah.Application.Dtos.Notifications;

namespace Tarabezah.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FloorplansController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FloorplansController> _logger;
    private readonly INotificationService _notificationService;

    public FloorplansController(
        IMediator mediator,
        ILogger<FloorplansController> logger,
        INotificationService notificationService)
    {
        _mediator = mediator;
        _logger = logger;
        _notificationService = notificationService;
    }

    [HttpGet("{guid}")]
    [ProducesResponseType(typeof(ApiResponse<FloorplanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FloorplanDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGuid(Guid guid)
    {
        var floorplan = await _mediator.Send(new GetFloorplanByIdQuery(guid));

        if (floorplan == null)
        {
            return NotFound(ApiResponse<FloorplanDto>.NotFound($"Floorplan with ID {guid} not found"));
        }

        return Ok(ApiResponse<FloorplanDto>.Success(floorplan, "Floorplan retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FloorplanDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<FloorplanDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateFloorplanCommand command)
    {
        try
        {
            var floorplan = await _mediator.Send(command);

            // Notify clients that a floorplan has been created
            await _notificationService.NotifyFloorplanCreatedAsync();

            return CreatedAtAction(
                nameof(GetByGuid),
                new { guid = floorplan.Guid },
                ApiResponse<FloorplanDto>.Created(floorplan, "Floorplan created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<FloorplanDto>.BadRequest("Failed to create floorplan", ex.Message));
        }
    }

    /// <summary>
    /// Gets all combined tables for a floorplan
    /// </summary>
    /// <param name="floorplanGuid">The GUID of the floorplan</param>
    /// <returns>A list of combined tables</returns>
    [HttpGet("{floorplanGuid}/combined-tables")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CombinedTableDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CombinedTableDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCombinedTables(Guid floorplanGuid)
    {
        _logger.LogInformation("Getting combined tables for floorplan {FloorplanGuid}", floorplanGuid);

        try
        {
            // First check if the floorplan exists
            var floorplan = await _mediator.Send(new GetFloorplanByIdQuery(floorplanGuid));
            if (floorplan == null)
            {
                return NotFound(ApiResponse<IEnumerable<CombinedTableDto>>.NotFound($"Floorplan with ID {floorplanGuid} not found"));
            }

            var result = await _mediator.Send(new GetCombinedTablesQuery { FloorplanGuid = floorplanGuid });

            return Ok(ApiResponse<IEnumerable<CombinedTableDto>>.Success(result, "Combined tables retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting combined tables for floorplan {FloorplanGuid}", floorplanGuid);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<CombinedTableDto>>.ServerError("An error occurred while retrieving combined tables"));
        }
    }

    /// <summary>
    /// Creates a new combined table by combining multiple tables in a floorplan
    /// </summary>
    /// <param name="floorplanGuid">The GUID of the floorplan</param>
    /// <param name="command">The command containing table details to combine</param>
    /// <remarks>
    /// The command should include:
    /// - List of FloorplanElementInstanceGuids to combine
    /// - Optional GroupName for the combined table
    /// - Optional MinCapacity (will be calculated from tables if not provided)
    /// - Optional MaxCapacity (will be calculated from tables if not provided)
    /// 
    /// The operation will fail if:
    /// - Any table doesn't exist
    /// - Any table belongs to a different floorplan
    /// - Any table is already part of another combined table
    /// </remarks>
    /// <returns>The created combined table details</returns>
    [HttpPost("{floorplanGuid}/combined-tables")]
    [ProducesResponseType(typeof(ApiResponse<CombinedTableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CombinedTableDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CombinedTableDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CombinedTableDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCombinedTable(Guid floorplanGuid, [FromBody] CreateCombinedTableCommand command)
    {
        if (command.FloorplanGuid != floorplanGuid)
        {
            return BadRequest(ApiResponse<CombinedTableDto>.BadRequest(
                "Invalid request",
                "Floorplan GUID in the route must match the GUID in the request body"));
        }

        if (!command.FloorplanElementInstanceGuids.Any())
        {
            return BadRequest(ApiResponse<CombinedTableDto>.BadRequest(
                "Invalid request",
                "At least one table must be specified to create a combined table"));
        }

        _logger.LogInformation(
            "Creating combined table for floorplan {FloorplanGuid} with {TableCount} tables",
            floorplanGuid,
            command.FloorplanElementInstanceGuids.Count);

        try
        {
            var result = await _mediator.Send(command);

            _logger.LogInformation(
                "Successfully created combined table {CombinedTableGuid} with {MemberCount} members",
                result.Guid,
                result.Members.Count);

            // Notify clients that a combined table has been created
            await _notificationService.NotifyCombinedTableCreatedAsync();

            return CreatedAtAction(
                nameof(GetCombinedTables),
                new { floorplanGuid },
                ApiResponse<CombinedTableDto>.Created(result, "Combined table created successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            return NotFound(ApiResponse<CombinedTableDto>.NotFound(ex.Message));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already part of"))
            {
            _logger.LogWarning(ex, "Conflict detected: {Message}", ex.Message);
            return Conflict(ApiResponse<CombinedTableDto>.Error(
                1,
                ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
            return BadRequest(ApiResponse<CombinedTableDto>.BadRequest(
                "Failed to create combined table",
                ex.Message));
            }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating combined table for floorplan {FloorplanGuid}", floorplanGuid);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<CombinedTableDto>.ServerError("An unexpected error occurred while creating the combined table"));
        }
    }

    /// <summary>
    /// Gets the floorplan for a specific restaurant, date and shift
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <param name="date">The date to get the floorplan for</param>
    /// <param name="shiftName">The name of the shift</param>
    /// <returns>The floorplan with element details and reservation status</returns>
    [HttpGet("{restaurantGuid}/by-date-shift")]
    [ProducesResponseType(typeof(ApiResponse<FloorplanByDateShiftResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FloorplanByDateShiftResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FloorplanByDateShiftResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFloorplanByDateAndShift(
        Guid restaurantGuid,
        [FromQuery] DateTime date,
        [FromQuery] string shiftName)
    {
        _logger.LogInformation(
            "Getting floorplan for restaurant {RestaurantGuid}, date {Date} and shift {ShiftName}",
            restaurantGuid,
            date.ToShortDateString(),
            shiftName);

        try
        {
            var query = new GetFloorplanByDateAndShiftQuery(restaurantGuid, date, shiftName);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<FloorplanByDateShiftResponseDto>.Success(result, "Floorplan retrieved successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters: {ErrorMessage}", ex.Message);
            return NotFound(ApiResponse<FloorplanByDateShiftResponseDto>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving floorplan: {ErrorMessage}", ex.Message);
            return BadRequest(ApiResponse<FloorplanByDateShiftResponseDto>.BadRequest("Failed to retrieve floorplan", ex.Message));
        }
    }

    /// <summary>
    /// Gets all combined tables for all floorplans of a restaurant
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <returns>A list of floorplans with their combined tables</returns>
    [HttpGet("restaurant/{restaurantGuid}/combined-tables")]
    [ProducesResponseType(typeof(ApiResponse<List<FloorplanCombinedTablesDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<FloorplanCombinedTablesDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRestaurantCombinedTables(Guid restaurantGuid)
    {
        _logger.LogInformation("Getting combined tables for restaurant {RestaurantGuid}", restaurantGuid);

        try
        {
            // First check if the restaurant exists
            var restaurant = await _mediator.Send(new GetRestaurantByIdQuery(restaurantGuid));
            if (restaurant == null)
            {
                return NotFound(ApiResponse<List<FloorplanCombinedTablesDto>>.NotFound($"Restaurant with ID {restaurantGuid} not found"));
            }

            var result = await _mediator.Send(new GetRestaurantCombinedTablesQuery { RestaurantGuid = restaurantGuid });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Combined tables retrieved successfully",
                ErrorMessage = (string?)null,
                ErrorDetails = (string[]?)null,
                Data = new { Result = result },
                IsSuccess = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting combined tables for restaurant {RestaurantGuid}", restaurantGuid);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An error occurred while retrieving combined tables",
                ErrorMessage = ex.Message,
                ErrorDetails = new[] { ex.StackTrace },
                Data = (object?)null,
                IsSuccess = false
            });
        }
    }
}