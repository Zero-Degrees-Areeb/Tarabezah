using Tarabezah.Domain.Common;

namespace Tarabezah.Domain.Entities;

/// <summary>
/// Represents a restaurant-shift association
/// </summary>
public class RestaurantShift : BaseEntity
{
    /// <summary>
    /// The ID of the restaurant
    /// </summary>
    public int RestaurantId { get; set; }
    
    /// <summary>
    /// Navigation property to the restaurant
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;
    
    /// <summary>
    /// The ID of the shift
    /// </summary>
    public int ShiftId { get; set; }
    
    /// <summary>
    /// Navigation property to the shift
    /// </summary>
    public Shift Shift { get; set; } = null!;
} 