using Tarabezah.Domain.Common;

namespace Tarabezah.Domain.Entities;

public class FloorplanElementInstance : BaseEntity
{
    public int FloorplanId { get; set; }
    public Floorplan Floorplan { get; set; } = null!;

    public int ElementId { get; set; }
    public Element Element { get; set; } = null!;
    public string? TableId { get; set; }
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }

    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Rotation { get; set; }
    public ICollection<CombinedTableMember> CombinedTableMemberships { get; set; } = new List<CombinedTableMember>();
} 