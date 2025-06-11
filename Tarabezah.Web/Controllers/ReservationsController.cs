using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Tarabezah.Application.Commands.AssignTableToReservation;
using Tarabezah.Application.Commands.CreateReservation;
using Tarabezah.Application.Commands.CreateWalkInReservation;
using Tarabezah.Application.Commands.UpdateReservationStatus;
using Tarabezah.Application.Common;
using Tarabezah.Application.Dtos;
using Tarabezah.Application.Dtos.Reservations;
using Tarabezah.Application.Queries.GetReservationByGuid;
using Tarabezah.Application.Queries.GetReservationsByDateAndShift;
using Tarabezah.Application.Queries.GetReservationAndCountByDateAndShift;
using Tarabezah.Application.Queries.GetWaitlistReservationByDateAndShift;
using Tarabezah.Application.Queries.GetReservationStatus;
using Tarabezah.Application.Commands.UpdateReservation;
using Tarabezah.Domain.Entities;
using Tarabezah.Application.Commands.UpdateAssignedTable;
using Tarabezah.Application.Commands.RemoveTableAssignment;
using Tarabezah.Application.Dtos.Clients;
using Tarabezah.Application.Commands.BlackListingClient;
using Tarabezah.Infrastructure.SignalR;
using Tarabezah.Application.Services;
using Tarabezah.Application.Dtos.Notifications;

namespace Tarabezah.Web.Controllers;

/// <summary>
/// Controller for reservation operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReservationsController> _logger;
    private readonly INotificationService _notificationService;

    public ReservationsController(
        IMediator mediator,
        ILogger<ReservationsController> logger,
        INotificationService notificationService)
    {
        _mediator = mediator;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Creates a new on-call reservation
    /// </summary>
    /// <param name="command">The reservation information</param>
    /// <returns>The created reservation</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command)
    {
        _logger.LogInformation("Received request to create reservation for client {ClientGuid} on {Date}",
            command.ClientGuid, command.Date.ToShortDateString());

        try
        {
            var result = await _mediator.Send(command);

            // Notify clients that a reservation has been created
            await _notificationService.NotifyReservationCreatedAsync();

            return Created($"api/reservations/{result.Guid}",
                ApiResponse<ReservationDto>.Created(result, "Reservation created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation: {ErrorMessage}", ex.Message);

            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<ReservationDto>.NotFound(ex.Message));
            }

            return BadRequest(ApiResponse<ReservationDto>.BadRequest("Failed to create reservation", ex.Message));
        }
    }

    /// <summary>
    /// Creates a new walk-in reservation with the current date and time
    /// </summary>
    /// <param name="command">The walk-in reservation information</param>
    /// <returns>The created reservation</returns>
    [HttpPost("walkin")]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateWalkInReservation([FromBody] CreateWalkInReservationCommand command)
    {
        _logger.LogInformation("Received request to create walk-in reservation for party size {PartySize}",
            command.PartySize);

        try
        {
            var result = await _mediator.Send(command);

            // Notify clients that a walk-in reservation has been created
            await _notificationService.NotifyReservationCreatedAsync();

            return Created($"api/reservations/{result.Guid}",
                ApiResponse<ReservationDto>.Created(result, "Walk-in reservation created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating walk-in reservation: {ErrorMessage}", ex.Message);

            // If client or shift was not found, return 404
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<ReservationDto>.NotFound(ex.Message));
            }

            return BadRequest(ApiResponse<ReservationDto>.BadRequest("Failed to create walk-in reservation", ex.Message));
        }
    }

    /// <summary>
    /// Assigns a table to a reservation
    /// </summary>
    /// <param name="reservationGuid">The GUID of the reservation</param>
    /// <param name="command">The table assignment information. Either FloorplanElementGuid or CombinedTableMemberGuid must be provided.</param>
    /// <returns>The updated reservation</returns>
    [HttpPost("{reservationGuid}/assign-table")]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTableToReservation(Guid reservationGuid, [FromBody] AssignTableToReservationCommand command)
    {
        // Ensure the route parameter matches the command property
        if (reservationGuid != command.ReservationGuid)
        {
            command.ReservationGuid = reservationGuid;
        }

        // Validate that at least one assignment type is provided
        if (!command.FloorplanElementGuid.HasValue && !command.CombinedTableMemberGuid.HasValue)
        {
            return BadRequest(ApiResponse<ReservationDto>.BadRequest("Either FloorplanElementGuid or CombinedTableMemberGuid must be provided"));
        }

        _logger.LogInformation("Received request to assign table to reservation {ReservationGuid}. FloorplanElementGuid: {FloorplanElementGuid}, CombinedTableMemberGuid: {CombinedTableMemberGuid}",
            command.ReservationGuid, command.FloorplanElementGuid, command.CombinedTableMemberGuid);

        try
        {
            var result = await _mediator.Send(command);

            // Notify clients that a table has been assigned to a reservation
            await _notificationService.NotifyTableAssignedAsync();

            return Ok(ApiResponse<ReservationDto>.Success(result, "Table assigned to reservation successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning table to reservation: {ErrorMessage}", ex.Message);

            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<ReservationDto>.NotFound(ex.Message));
            }

            return BadRequest(ApiResponse<ReservationDto>.BadRequest("Failed to assign table to reservation", ex.Message));
        }
    }

    /// <summary>
    /// Gets reservations grouped by status for a specific restaurant, date and shift
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <param name="date">The date to get reservations for</param>
    /// <param name="shiftName">The name of the shift</param>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The page size for pagination (default: 10)</param>
    /// <param name="searchName">Optional search term for client name or phone number</param>
    /// <param name="tags">Optional list of reservation tags to filter by</param>
    /// <param name="minPartySize">Optional minimum party size filter</param>
    /// <param name="maxPartySize">Optional maximum party size filter</param>
    /// <param name="startTime">Optional start time filter (inclusive)</param>
    /// <param name="endTime">Optional end time filter (inclusive)</param>
    /// <param name="statuses">Optional comma-separated list of reservation statuses to filter by (e.g. "Confirmed,Seated,Upcoming")</param>
    /// <param name="sortBy">Optional field to sort by (clientname, time)</param>
    /// <returns>A paginated list of reservations grouped by status</returns>
    [HttpGet("{restaurantGuid}/by-date-shift")]
    [ProducesResponseType(typeof(ApiResponse<ReservationGroupsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReservationGroupsResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ReservationGroupsResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservationsByDateAndShift(
        Guid restaurantGuid,
        [FromQuery] DateTime date,
        [FromQuery] string shiftName,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchName = null,
        [FromQuery] List<string>? tags = null,
        [FromQuery] int? minPartySize = null,
        [FromQuery] int? maxPartySize = null,
        [FromQuery] TimeSpan? startTime = null,
        [FromQuery] TimeSpan? endTime = null,
        [FromQuery] string? statuses = null,
        [FromQuery] string? sortBy = null)
    {
        _logger.LogInformation(
            "Getting reservations for restaurant {RestaurantGuid}, date {Date}, shift {ShiftName}, page {Page} with size {Size}",
            restaurantGuid,
            date.ToShortDateString(),
            shiftName,
            pageNumber,
            pageSize);

        try
        {
            // Parse the comma-separated statuses into a list of ReservationStatus
            List<ReservationStatus>? statusList = null;
            if (!string.IsNullOrWhiteSpace(statuses))
            {
                try
                {
                    statusList = statuses.Split(',')
                        .Select(s => (ReservationStatus)Enum.Parse(typeof(ReservationStatus), s.Trim()))
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invalid reservation status values provided: {Statuses}", statuses);
                    return BadRequest(ApiResponse<ReservationGroupsResponseDto>.BadRequest(
                        "Invalid reservation status values",
                        "One or more status values are not valid. Valid values are: Confirmed, Seated, Completed, Rejected, NoShow, Upcoming, Cancelled"));
                }
            }

            var query = new GetReservationsByDateAndShiftQuery(
                restaurantGuid,
                date,
                shiftName,
                pageNumber,
                pageSize,
                searchName,
                tags,
                minPartySize,
                maxPartySize,
                startTime,
                endTime,
                statusList,
                sortBy);

            var result = await _mediator.Send(query);

            return Ok(ApiResponse<ReservationGroupsResponseDto>.Success(result.Data, "Reservations retrieved successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters: {ErrorMessage}", ex.Message);
            return NotFound(ApiResponse<ReservationGroupsResponseDto>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations: {ErrorMessage}", ex.Message);
            return BadRequest(ApiResponse<ReservationGroupsResponseDto>.BadRequest("Failed to retrieve reservations", ex.Message));
        }
    }

    /// <summary>
    /// Gets reservations with counts grouped by status for a specific restaurant, date and shift
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <param name="date">The date to get reservations for</param>
    /// <param name="shiftName">The name of the shift</param>
    /// <returns>Grouped reservations with summary counts</returns>
    [HttpGet("{restaurantGuid}/statuscount-by-date-shift")]
    [ProducesResponseType(typeof(ApiResponse<ReservationCountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReservationCountDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ReservationCountDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservationAndCountByDateAndShift(
        Guid restaurantGuid,
        [FromQuery] DateTime date,
        [FromQuery] string shiftName)
    {
        _logger.LogInformation(
            "Getting reservation counts for restaurant {RestaurantGuid}, date {Date} and shift {ShiftName}",
            restaurantGuid,
            date.ToShortDateString(),
            shiftName);

        try
        {
            var query = new GetReservationAndCountByDateAndShiftQuery(restaurantGuid, date, shiftName);
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<ReservationCountDto>.Success(result, "Reservations retrieved successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters: {ErrorMessage}", ex.Message);
            return NotFound(ApiResponse<ReservationCountDto>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations: {ErrorMessage}", ex.Message);
            return BadRequest(ApiResponse<ReservationCountDto>.BadRequest("Failed to retrieve reservations", ex.Message));
        }
    }

    /// <summary>
    /// Gets all waitlist reservations (reservations without tables) for a specific restaurant, date and shift
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <param name="date">The date to get reservations for</param>
    /// <param name="shiftName">The name of the shift</param>
    /// <param name="pageNumber">The page number for pagination (default: 1)</param>
    /// <param name="pageSize">The page size for pagination (default: 10)</param>
    /// <param name="searchName">Optional search term for client name or phone number</param>
    /// <param name="tags">Optional list of reservation tags to filter by</param>
    /// <param name="minPartySize">Optional minimum party size filter</param>
    /// <param name="maxPartySize">Optional maximum party size filter</param>
    /// <param name="startTime">Optional start time filter (inclusive)</param>
    /// <param name="endTime">Optional end time filter (inclusive)</param>
    /// <param name="sortBy">Optional field to sort by (clientname, time)</param>
    /// <returns>A paginated list of waitlist reservations</returns>
    [HttpGet("{restaurantGuid}/waitlist")]
    [ProducesResponseType(typeof(ApiResponse<WaitlistReservationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WaitlistReservationResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<WaitlistReservationResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWaitlistReservationByDateAndShift(
        Guid restaurantGuid,
        [FromQuery] DateTime date,
        [FromQuery] string shiftName,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchName = null,
        [FromQuery] List<string>? tags = null,
        [FromQuery] int? minPartySize = null,
        [FromQuery] int? maxPartySize = null,
        [FromQuery] TimeSpan? startTime = null,
        [FromQuery] TimeSpan? endTime = null,
        [FromQuery] string? sortBy = null)
    {
        _logger.LogInformation(
            "Getting waitlist reservations for restaurant {RestaurantGuid}, date {Date} and shift {ShiftName}",
            restaurantGuid,
            date.ToShortDateString(),
            shiftName);

        try
        {
            var query = new GetWaitlistReservationByDateAndShiftQuery(
                restaurantGuid,
                date,
                shiftName,
                pageNumber,
                pageSize,
                searchName,
                tags,
                minPartySize,
                maxPartySize,
                startTime,
                endTime,
                sortBy);

            var result = await _mediator.Send(query);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Waitlist reservations retrieved successfully",
                ErrorMessage = (string?)null,
                ErrorDetails = (string?)null,
                Result = result,
                IsSuccess = true
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters: {ErrorMessage}", ex.Message);
            return NotFound(new
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Not Found",
                ErrorMessage = ex.Message,
                ErrorDetails = (string?)null,
                Result = (object?)null,
                IsSuccess = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving waitlist reservations: {ErrorMessage}", ex.Message);
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Bad Request",
                ErrorMessage = "Failed to retrieve waitlist reservations",
                ErrorDetails = ex.Message,
                Result = (object?)null,
                IsSuccess = false
            });
        }
    }

    /// <summary>
    /// Updates the status of a reservation
    /// </summary>
    /// <param name="reservationGuid">The GUID of the reservation</param>
    /// <param name="command">The status update information</param>
    /// <returns>The updated reservation</returns>
    [HttpPatch("{reservationGuid}/status")]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReservationStatus(Guid reservationGuid, [FromBody] UpdateReservationStatusCommand command)
    {
        // Ensure the route parameter matches the command property
        if (reservationGuid != command.ReservationGuid)
        {
            command.ReservationGuid = reservationGuid;
        }

        _logger.LogInformation("Received request to update status for reservation {ReservationGuid} to {NewStatus}",
            command.ReservationGuid, command.NewStatus);

        try
        {
            var result = await _mediator.Send(command);

            // Notify clients that a reservation status has been updated
            await _notificationService.NotifyReservationUpdatedAsync();

            return Ok(ApiResponse<ReservationDto>.Success(result, "Reservation status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation status: {ErrorMessage}", ex.Message);

            // Return 404 if reservation not found
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<ReservationDto>.NotFound(ex.Message));
            }

            return BadRequest(ApiResponse<ReservationDto>.BadRequest("Failed to update reservation status", ex.Message));
        }
    }

    /// <summary>
    /// Gets detailed information for a specific reservation by its GUID
    /// </summary>
    /// <param name="reservationGuid">The GUID of the reservation to retrieve</param>
    /// <param name="restaurantGuid">Optional GUID of the restaurant to check blacklist status</param>
    /// <returns>The reservation details including client information and blacklist status</returns>
    [HttpGet("{reservationGuid}")]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservationByGuid(Guid reservationGuid, [FromQuery] Guid? restaurantGuid = null)
    {
        _logger.LogInformation("Getting reservation details for GUID: {ReservationGuid} with restaurant check: {RestaurantGuid}",
            reservationGuid,
            restaurantGuid);

        try
        {
            var query = new GetReservationByGuidQuery(reservationGuid, restaurantGuid);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<ReservationDto>.Success(result, "Reservation details retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservation details: {ErrorMessage}", ex.Message);

            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<ReservationDto>.NotFound(ex.Message));
            }

            // Return 400 for other errors
            return BadRequest(ApiResponse<ReservationDto>.BadRequest("Failed to retrieve reservation details", ex.Message));
        }
    }

    /// <summary>
    /// Gets all available reservation statuses
    /// </summary>
    /// <returns>List of reservation statuses with their values and names</returns>
    [HttpGet("statuses")]
    [ProducesResponseType(typeof(ApiResponse<List<ReservationStatusDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservationStatuses()
    {
        _logger.LogInformation("Getting all reservation statuses");

        try
        {
            var query = new GetReservationStatusQuery();
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<List<ReservationStatusDto>>.Success(result, "Reservation statuses retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation statuses: {ErrorMessage}", ex.Message);
            return BadRequest(ApiResponse<List<ReservationStatusDto>>.BadRequest("Failed to retrieve reservation statuses", ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing reservation
    /// </summary>
    /// <param name="command">The update reservation command</param>
    /// <returns>The updated reservation details</returns>
    [HttpPost("update")]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> UpdateReservation(UpdateReservationCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            // Notify clients that a reservation has been updated
            await _notificationService.NotifyReservationUpdatedAsync();

            return Ok(ApiResponse<ReservationDto>.Success(result, "Reservation updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation: {ErrorMessage}", ex.Message);
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<ReservationDto>.NotFound(ex.Message));
            }
            return BadRequest(ApiResponse<ReservationDto>.BadRequest("Failed to update reservation", ex.Message));
        }
    }

    /// <summary>
    /// Updates the assigned table for a reservation
    /// </summary>
    /// <param name="reservationGuid">The GUID of the reservation</param>
    /// <param name="command">The command containing the new table assignment</param>
    /// <returns>The updated reservation</returns>
    [HttpPost("{reservationGuid}/change-assign-table")]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAssignedTable(Guid reservationGuid, [FromBody] UpdateAssignedTableCommand command)
    {
        // Ensure the route parameter matches the command property
        if (reservationGuid != command.ReservationGuid)
        {
            command.ReservationGuid = reservationGuid;
        }

        // Validate that at least one assignment type is provided
        if (!command.FloorplanElementGuid.HasValue && !command.CombinedTableMemberGuid.HasValue)
        {
            return BadRequest(ApiResponse<ReservationDto>.BadRequest("Either FloorplanElementGuid or CombinedTableMemberGuid must be provided"));
        }

        _logger.LogInformation("Received request to update table assignment for reservation {ReservationGuid}. FloorplanElementGuid: {FloorplanElementGuid}, CombinedTableMemberGuid: {CombinedTableMemberGuid}",
            command.ReservationGuid, command.FloorplanElementGuid, command.CombinedTableMemberGuid);

        try
        {
            var result = await _mediator.Send(command);

            // Notify clients that a table assignment has been updated
            await _notificationService.NotifyTableAssignedAsync();

            return Ok(ApiResponse<ReservationDto>.Success(result, "Table assignment updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table assignment for reservation: {ErrorMessage}", ex.Message);

            // Return 404 if reservation, floorplan element, or combined table member not found
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<ReservationDto>.NotFound(ex.Message));
            }

            // Return 400 for other errors
            return BadRequest(ApiResponse<ReservationDto>.BadRequest("Failed to update table assignment", ex.Message));
        }
    }

    /// <summary>
    /// Removes a table assignment from a reservation
    /// </summary>
    /// <param name="reservationGuid">The GUID of the reservation</param>
    /// <returns>The updated reservation</returns>
    [HttpPost("{reservationGuid}/remove-table")]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ReservationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTableAssignment(Guid reservationGuid)
    {
        _logger.LogInformation("Received request to remove table from reservation {ReservationGuid}", reservationGuid);

        try
        {
            var command = new RemoveTableAssignmentCommand { ReservationGuid = reservationGuid };
            var result = await _mediator.Send(command);

            // Notify clients that a table has been unassigned from a reservation
            await _notificationService.NotifyTableUnassignedAsync();

            return Ok(ApiResponse<ReservationDto>.Success(result, "Table removed from reservation successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing table from reservation: {ErrorMessage}", ex.Message);

            // Return 404 if reservation not found
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<ReservationDto>.NotFound(ex.Message));
            }

            // Return 400 for other errors
            return BadRequest(ApiResponse<ReservationDto>.BadRequest("Failed to remove table from reservation", ex.Message));
        }
    }

    /// <summary>
    /// Blocks or unblocks a client from making reservations at a specific restaurant
    /// </summary>
    /// <param name="command">The command containing client and restaurant information</param>
    /// <returns>The blocking record details</returns>
    [HttpPost("black-list")]
    [ProducesResponseType(typeof(ApiResponse<BlackListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BlackListDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BlackListDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BlockClient([FromBody] BlackListngClientCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            // Notify clients that a client has been blocked
            await _notificationService.NotifyClientBlockedAsync();

            return Ok(ApiResponse<BlackListDto>.Success(result, "Client blocked successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting client: {ErrorMessage}", ex.Message);

            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<BlackListDto>.NotFound(ex.Message));
            }

            return BadRequest(ApiResponse<BlackListDto>.BadRequest("Failed to blacklist client", ex.Message));
        }
    }
}