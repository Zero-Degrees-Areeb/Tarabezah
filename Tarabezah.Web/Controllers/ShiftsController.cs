using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tarabezah.Application.Common;
using Tarabezah.Application.Dtos;
using Tarabezah.Application.Queries.GetRestaurantShifts;
using Tarabezah.Application.Queries.GetShiftTimeDuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Tarabezah.Infrastructure.SignalR;

namespace Tarabezah.Web.Controllers;

/// <summary>
/// Controller for managing restaurant shifts
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ShiftsController> _logger;
    private readonly IHubContext<TarabezahHub> _hubContext;

    public ShiftsController(
        IMediator mediator,
        ILogger<ShiftsController> logger,
        IHubContext<TarabezahHub> hubContext)
    {
        _mediator = mediator;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Get all shifts for a specific restaurant
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <returns>List of shifts for the specified restaurant</returns>
    [HttpGet("restaurant/{restaurantGuid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShiftDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShiftDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShiftsByRestaurantGuid(Guid restaurantGuid)
    {
        _logger.LogInformation("Getting shifts for restaurant with GUID: {RestaurantGuid}", restaurantGuid);

        var query = new GetRestaurantShiftsQuery { RestaurantGuid = restaurantGuid };
        var result = await _mediator.Send(query);

        if (result == null || !result.Any())
        {
            return NotFound(ApiResponse<IEnumerable<ShiftDto>>.NotFound($"No shifts found for restaurant with ID {restaurantGuid}"));
        }

        return Ok(ApiResponse<IEnumerable<ShiftDto>>.Success(result, "Shifts retrieved successfully"));
    }

    /// <summary>
    /// Get time duration information for a specific shift in a restaurant
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <param name="shiftGuid">The GUID of the shift</param>
    /// <param name="partySize">The size of the party to check table availability for</param>
    /// <param name="date">The date to check table availability for</param>
    /// <param name="tableType">The type of table to check availability for (optional)</param>
    /// <returns>List of time slots with availability information</returns>
    [HttpGet("time-duration")]
    [ProducesResponseType(typeof(ApiResponse<List<TimeSlotDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<TimeSlotDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShiftTimeDuration([FromQuery] GetShiftTimeDurationQuery query)
    {
        _logger.LogInformation("Getting shift time duration for restaurant {RestaurantGuid}, shift {ShiftGuid} with party size {PartySize} for date {Date}",
            query.RestaurantGuid, query.ShiftGuid, query.PartySize, query.Date);

        try
        {
            var result = await _mediator.Send(query);

            // Always return OK with the result, even if empty
            return Ok(ApiResponse<List<TimeSlotDto>>.Success(result ?? new List<TimeSlotDto>(),
                result?.Any() == true ? "Time slots retrieved successfully" : "No time slots available for the given Table Type"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting time slots: {ErrorMessage}", ex.Message);
            if (ex is InvalidOperationException)
            {
                return NotFound(ApiResponse<List<TimeSlotDto>>.NotFound(ex.Message));
            }
            return BadRequest(ApiResponse<List<TimeSlotDto>>.BadRequest("Failed to get time slots", ex.Message));
        }
    }
}