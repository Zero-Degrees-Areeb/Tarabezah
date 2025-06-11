using System;
using System.Collections.Generic;

namespace Tarabezah.Application.Dtos;

/// <summary>
/// Data transfer object for floorplan combined tables response
/// </summary>
public class FloorplanCombinedTablesDto
{
    /// <summary>
    /// The GUID of the floorplan
    /// </summary>
    public Guid FloorplanGuid { get; set; }

    /// <summary>
    /// The name of the floorplan
    /// </summary>
    public string FloorPlanName { get; set; } = string.Empty;

    /// <summary>
    /// The combined tables for this floorplan
    /// </summary>
    public List<CombinedTableDto> FloorplanCombinedTable { get; set; } = new List<CombinedTableDto>();
}