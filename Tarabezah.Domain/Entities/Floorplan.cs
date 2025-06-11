using Tarabezah.Domain.Common;

namespace Tarabezah.Domain.Entities;

public class Floorplan : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public ICollection<FloorplanElementInstance> Elements { get; set; } = new List<FloorplanElementInstance>();
    
    public ICollection<CombinedTable> CombinedTables { get; set; } = new List<CombinedTable>();
} 