using System;
using System.Collections.Generic;

namespace Tarabezah.Application.Dtos;

/// <summary>
/// Data transfer object for lookup data (enums and predefined values)
/// </summary>
public class LookupDataDto
{
    /// <summary>
    /// List of client sources with their name and value
    /// </summary>
    public List<EnumValueDto> ClientSources { get; set; } = new List<EnumValueDto>();
    
    /// <summary>
    /// List of client tags with their name and value
    /// </summary>
    public List<EnumValueDto> ClientTags { get; set; } = new List<EnumValueDto>();
    
    /// <summary>
    /// List of table types with their name and value
    /// </summary>
    public List<EnumValueDto> TableTypes { get; set; } = new List<EnumValueDto>();
    
    /// <summary>
    /// List of element purposes with their name and value
    /// </summary>
    public List<EnumValueDto> ElementPurposes { get; set; } = new List<EnumValueDto>();
    
    /// <summary>
    /// List of shifts for a specific restaurant
    /// </summary>
    public List<ShiftDto> RestaurantShifts { get; set; } = new List<ShiftDto>();
} 