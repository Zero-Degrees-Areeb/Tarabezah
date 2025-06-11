using Tarabezah.Domain.Common;

namespace Tarabezah.Domain.Entities;

/// <summary>
/// Represents a restaurant shift like Lunch or Dinner
/// </summary>
public class Shift : BaseEntity
{
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
    /// Collection of restaurant-shift associations
    /// </summary>
    public ICollection<RestaurantShift> RestaurantShifts { get; set; } = new List<RestaurantShift>();
    
    /// <summary>
    /// Collection of reservations for this shift
    /// </summary>
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
} 