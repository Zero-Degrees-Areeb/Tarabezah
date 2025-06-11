using System;

namespace Tarabezah.Application.Dtos;

/// <summary>
/// Data transfer object for Shift information
/// </summary>
public class ShiftDto
{
    /// <summary>
    /// The unique identifier for the shift
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The globally unique identifier for the shift
    /// </summary>
    public Guid Guid { get; set; }
    
    /// <summary>
    /// The name of the shift (e.g., Lunch, Dinner)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The start time of the shift
    /// </summary>
    public TimeSpan StartTime { get; set; }
    
    /// <summary>
    /// The end time of the shift
    /// </summary>
    public TimeSpan EndTime { get; set; }
    
    /// <summary>
    /// When the shift record was created
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// When the shift record was last modified
    /// </summary>
    public DateTime ModifiedDate { get; set; }
} 