using Tarabezah.Domain.Common;

namespace Tarabezah.Domain.Entities;

/// <summary>
/// Represents a group of combined tables on a floorplan
/// </summary>
public class CombinedTable : BaseEntity
{
    /// <summary>
    /// The ID of the floorplan this combined table belongs to
    /// </summary>
    public int FloorplanId { get; set; }

    /// <summary>
    /// Navigation property to the floorplan
    /// </summary>
    public Floorplan Floorplan { get; set; } = null!;

    /// <summary>
    /// Optional name for identifying this combined table setup
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// Minimum seating capacity of the combined table
    /// </summary>
    public int? MinCapacity { get; set; }

    /// <summary>
    /// Maximum seating capacity of the combined table
    /// </summary>
    public int? MaxCapacity { get; set; }

    /// <summary>
    /// Collection of member tables that make up this combined table
    /// </summary>
    public ICollection<CombinedTableMember> Members { get; set; } = new List<CombinedTableMember>();
}