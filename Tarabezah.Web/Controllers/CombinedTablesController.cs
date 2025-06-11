using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Commands.DeleteCombinedTable;
using Tarabezah.Application.Common;
using Tarabezah.Application.Dtos.Notifications;
using Tarabezah.Application.Services;

namespace Tarabezah.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CombinedTablesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CombinedTablesController> _logger;
    private readonly INotificationService _notificationService;

    public CombinedTablesController(
        IMediator mediator,
        ILogger<CombinedTablesController> logger,
        INotificationService notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <summary>
    /// Deletes a combined table
    /// </summary>
    /// <param name="combinedTableGuid">The GUID of the combined table to delete</param>
    /// <returns>NoContent if successful</returns>
    [HttpDelete("{combinedTableGuid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCombinedTable(Guid combinedTableGuid)
    {
        _logger.LogInformation("Deleting combined table {CombinedTableGuid}", combinedTableGuid);

        try
        {
            var command = new DeleteCombinedTableCommand { CombinedTableGuid = combinedTableGuid };
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(ApiResponse<object>.NotFound($"Combined table with ID {combinedTableGuid} not found"));
            }

            // Notify clients that a combined table has been deleted
            await _notificationService.NotifyCombinedTableDeletedAsync();

            // For 204 No Content responses, we don't actually send content, but define the response type for documentation
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting combined table {CombinedTableGuid}", combinedTableGuid);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ServerError("An error occurred while deleting the combined table"));
        }
    }
}