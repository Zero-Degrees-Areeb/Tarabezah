using Microsoft.AspNetCore.Mvc;
using MediatR;
using Tarabezah.Application.Common;
using Tarabezah.Application.Queries.GetEnumValues;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnumsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EnumsController> _logger;

    public EnumsController(
        IMediator mediator,
        ILogger<EnumsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("tableTypes")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTableTypes()
    {
        _logger.LogInformation("Getting all table types");
        var tableTypes = await _mediator.Send(new GetTableTypesQuery());
        return Ok(ApiResponse<IEnumerable<string>>.Success(tableTypes, "Table types retrieved successfully"));
    }

    [HttpGet("elementPurposes")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetElementPurposes()
    {
        _logger.LogInformation("Getting all element purposes");
        var purposes = await _mediator.Send(new GetElementPurposesQuery());
        return Ok(ApiResponse<IEnumerable<string>>.Success(purposes, "Element purposes retrieved successfully"));
    }
    
    /// <summary>
    /// Gets all lookup data including client sources, tags, table types, and restaurant shifts
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant to get shifts for</param>
    /// <returns>Combined lookup data</returns>
    [HttpGet("lookupData/{restaurantGuid}")]
    [ProducesResponseType(typeof(ApiResponse<LookupDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LookupDataDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLookupData(Guid restaurantGuid)
    {
        _logger.LogInformation("Getting lookup data for restaurant with GUID: {RestaurantGuid}", restaurantGuid);
        
        try
        {
            var query = new GetLookupDataQuery { RestaurantGuid = restaurantGuid };
            var result = await _mediator.Send(query);
            
            return Ok(ApiResponse<LookupDataDto>.Success(result, "Lookup data retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lookup data for restaurant {RestaurantGuid}: {ErrorMessage}", 
                restaurantGuid, ex.Message);
            
            return NotFound(ApiResponse<LookupDataDto>.NotFound($"Error retrieving lookup data. Details: {ex.Message}"));
        }
    }
} 