using Tarabezah.Domain.Common;

namespace Tarabezah.Domain.Entities;

public class Restaurant : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Floorplan> Floorplans { get; set; } = new List<Floorplan>();

    public ICollection<RestaurantShift> RestaurantShifts { get; set; } = new List<RestaurantShift>();

    /// <summary>
    /// Collection of clients that have been blocked by this restaurant
    /// </summary>
    public ICollection<BlackList> BlockedClients { get; set; } = new List<BlackList>();
}