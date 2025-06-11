using Microsoft.AspNetCore.Mvc;
using Tarabezah.Application.Commands.CreateFloorplanElement;
using Tarabezah.Application.Commands.DeleteFloorplanElement;
using Tarabezah.Application.Commands.UpdateFloorplanElement;
using Tarabezah.Application.Common;
using Tarabezah.Application.Dtos;
using Tarabezah.Application.Queries.GetFloorplanElementById;
using Tarabezah.Application.Queries.GetFloorplanElements;
using MediatR;
using Tarabezah.Application.Queries.GetBlockTableByFloorplanInstanceGuid;
using Tarabezah.Application.Commands.BlockingTable;
using Microsoft.AspNetCore.SignalR;
using Tarabezah.Infrastructure.SignalR;
using Tarabezah.Application.Services;

namespace Tarabezah.Web.Controllers;

[ApiController]
[Route("api/floorplans/{floorplanGuid}/elements")]
public class FloorplanElementsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FloorplanElementsController> _logger;
    private readonly INotificationService _notificationService;

    public FloorplanElementsController(
        IMediator mediator,
        ILogger<FloorplanElementsController> logger,
        INotificationService notificationService)
    {
        _mediator = mediator;
        _logger = logger;
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FloorplanElementResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FloorplanElementResponseDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetElements(Guid floorplanGuid)
    {
        var elements = await _mediator.Send(new GetFloorplanElementsQuery(floorplanGuid));

        if (elements == null)
        {
            return NotFound(ApiResponse<IEnumerable<FloorplanElementResponseDto>>.NotFound($"Floorplan with ID {floorplanGuid} not found"));
        }

        return Ok(ApiResponse<IEnumerable<FloorplanElementResponseDto>>.Success(elements, "Floorplan elements retrieved successfully"));
    }

    [HttpGet("{guid}")]
    [ProducesResponseType(typeof(ApiResponse<FloorplanElementDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FloorplanElementDetailResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetElement(Guid floorplanGuid, Guid guid)
    {
        var element = await _mediator.Send(new GetFloorplanElementByIdQuery(floorplanGuid, guid));

        if (element == null)
        {
            return NotFound(ApiResponse<FloorplanElementDetailResponseDto>.NotFound($"Element with ID {guid} not found in floorplan {floorplanGuid}"));
        }

        return Ok(ApiResponse<FloorplanElementDetailResponseDto>.Success(element, "Floorplan element retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddElement(Guid floorplanGuid, [FromBody] CreateFloorplanElementCommand command)
    {
        try
        {
            // Update the command with the floorplan GUID from the route
            var commandWithFloorplan = command with { FloorplanGuid = floorplanGuid };

            var guid = await _mediator.Send(commandWithFloorplan);

            // Notify clients that an element has been created in this floorplan
            await _notificationService.NotifyTableCreatedAsync();

            var response = ApiResponse<Guid>.Created(guid, "Floorplan element added successfully");
            return CreatedAtAction(nameof(GetElement), new { floorplanGuid, guid }, response);
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<Guid>.NotFound("Resource not found", ex.Message));
            }
            return BadRequest(ApiResponse<Guid>.BadRequest("Failed to add element to floorplan", ex.Message));
        }
    }

    [HttpPut("{guid}")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateElement(Guid floorplanGuid, Guid guid, [FromBody] UpdateFloorplanElementCommand command)
    {
        try
        {
            // Update the command with the floorplan and element instance GUIDs from the route
            var commandWithIds = command with { FloorplanGuid = floorplanGuid, ElementInstanceGuid = guid };

            var resultGuid = await _mediator.Send(commandWithIds);

            // Notify clients that an element has been updated
            await _notificationService.NotifyTableUpdatedAsync();

            var response = ApiResponse<Guid>.Success(resultGuid, "Floorplan element updated successfully");
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<Guid>.NotFound("Resource not found", ex.Message));
            }
            return BadRequest(ApiResponse<Guid>.BadRequest("Failed to update element in floorplan", ex.Message));
        }
    }

    [HttpDelete("{guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteElement(Guid floorplanGuid, Guid guid)
    {
        var result = await _mediator.Send(new DeleteFloorplanElementCommand(floorplanGuid, guid));

        if (!result)
        {
            return NotFound(ApiResponse<bool>.NotFound($"Element with ID {guid} not found in floorplan {floorplanGuid}"));
        }

        // Notify clients that an element has been deleted
        await _notificationService.NotifyTableDeletedAsync();

        return Ok(ApiResponse<bool>.Success(true, "Floorplan element deleted successfully"));
    }

    [HttpPost("block-table")]
    [ProducesResponseType(typeof(ApiResponse<BlockTableResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<BlockTableResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BlockTableResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BlockTable(Guid floorplanGuid, [FromBody] BlockingTableCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            // Notify clients that a table has been blocked
            // We need to load the floorplan element instance to get its floorplan GUID
            var elementInstance = await _mediator.Send(
                new GetFloorplanElementByIdQuery(floorplanGuid, result.FloorplanElementInstanceGuid));

            if (elementInstance != null)
            {
                await _notificationService.NotifyTableStatusChangedAsync();
            }

            var response = ApiResponse<BlockTableResponse>.Created(result, "Table blocked successfully");
            return CreatedAtAction(nameof(GetElement),
                new { floorplanGuid = floorplanGuid, guid = result.BlockTableGuid },
                response);
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ApiResponse<BlockTableResponse>.NotFound("Resource not found", ex.Message));
            }
            return BadRequest(ApiResponse<BlockTableResponse>.BadRequest("Failed to block table", ex.Message));
        }
    }

    [HttpGet("block-table")]
    [ProducesResponseType(typeof(ApiResponse<BlockTableDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BlockTableDetailsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<BlockTableDetailsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBlockTableByFloorplanInstanceGuid([FromQuery] Guid floorplanElementInstanceGuid)
    {
        try
        {
            var query = new GetBlockTableByFloorplanInstanceGuidQuery(floorplanElementInstanceGuid);
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<BlockTableDetailsDto>.Success(result, "Block table information retrieved successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            return NotFound(ApiResponse<BlockTableDetailsDto>.NotFound("Resource not found", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving block table information for element {ElementGuid}", floorplanElementInstanceGuid);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<BlockTableDetailsDto>.ServerError("An error occurred while retrieving block table information"));
        }
    }
}