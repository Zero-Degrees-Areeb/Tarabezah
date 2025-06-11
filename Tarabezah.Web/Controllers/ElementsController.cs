using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tarabezah.Application.Commands.CreateElement;
using Tarabezah.Application.Commands.CreateElementWithImage;
using Tarabezah.Application.Common;
using Tarabezah.Application.Dtos;
using Tarabezah.Application.Queries.GetAllElements;
using Tarabezah.Application.Dtos.Notifications;
using Tarabezah.Application.Queries.GetElementById;
using Tarabezah.Application.Services;

namespace Tarabezah.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ElementsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ElementsController> _logger;
    private readonly INotificationService _notificationService;

    public ElementsController(
        IMediator mediator,
        ILogger<ElementsController> logger,
        INotificationService notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ElementResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var elements = await _mediator.Send(new GetAllElementsQuery());
        return Ok(ApiResponse<IEnumerable<ElementResponseDto>>.Success(elements, "Elements retrieved successfully"));
    }

    [HttpGet("{guid}")]
    [ProducesResponseType(typeof(ApiResponse<ElementResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ElementResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGuid(Guid guid)
    {
        var element = await _mediator.Send(new GetElementByIdQuery(guid));

        if (element == null)
        {
            return NotFound(ApiResponse<ElementResponseDto>.NotFound($"Element with ID {guid} not found"));
        }

        return Ok(ApiResponse<ElementResponseDto>.Success(element, "Element retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateElementCommand command)
    {
        try
        {
            var guid = await _mediator.Send(command);

            // Notify clients that an element has been created
            await _notificationService.NotifyTableCreatedAsync();

            var response = ApiResponse<Guid>.Created(guid, "Element created successfully");
            return CreatedAtAction(nameof(GetByGuid), new { guid }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<Guid>.BadRequest("Failed to create element", ex.Message));
        }
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWithImage([FromForm] CreateElementWithImageCommand command)
    {
        try
        {
            var guid = await _mediator.Send(command);

            // Notify clients that an element with image has been created
            await _notificationService.NotifyTableCreatedAsync();

            var response = ApiResponse<Guid>.Created(guid, "Element with image created successfully");
            return CreatedAtAction(nameof(GetByGuid), new { guid }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<Guid>.BadRequest("Failed to create element with image", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating element with image upload");
            return BadRequest(ApiResponse<Guid>.BadRequest("Failed to create element", "An error occurred processing the image"));
        }
    }
}