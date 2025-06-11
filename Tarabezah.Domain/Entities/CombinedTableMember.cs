using Tarabezah.Domain.Common;

namespace Tarabezah.Domain.Entities;

/// <summary>
/// Represents a FloorplanElementInstance that is part of a combined table
/// </summary>
public class CombinedTableMember : BaseEntity
{
    /// <summary>
    /// The ID of the combined table this member belongs to
    /// </summary>
    public int CombinedTableId { get; set; }

    /// <summary>
    /// Navigation property to the combined table
    /// </summary>
    public CombinedTable CombinedTable { get; set; } = null!;

    /// <summary>
    /// The ID of the floorplan element instance that is part of the combined table
    /// </summary>
    public int FloorplanElementInstanceId { get; set; }

    /// <summary>
    /// Navigation property to the floorplan element instance
    /// </summary>
    public FloorplanElementInstance FloorplanElementInstance { get; set; } = null!;

    /// <summary>
    /// Collection of reservations assigned to this combined table member
    /// </summary>
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}