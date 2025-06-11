using System;

namespace Tarabezah.Application.Dtos;

public class FloorplanElementResponseDto
{
    public Guid Guid { get; set; } // The unique identifier for this floorplan element instance
    public string? TableId { get; set; } = string.Empty; // Unique identifier for this table within the floorplan
    public Guid ElementGuid { get; set; } // Reference to the element type (table, chair, etc.)
    public string ElementName { get; set; } = string.Empty; // Name of the element type
    public string ElementImageUrl { get; set; } = string.Empty; // Image URL of the element type
    public string ElementType { get; set; } = string.Empty; // Type of element (e.g., "Table", "Chair")
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public double X { get; set; } // X position on the floorplan
    public double Y { get; set; } // Y position on the floorplan
    public double Width { get; set; } // Width of the element
    public double Height { get; set; } // Height of the element
    public double Rotation { get; set; } // Rotation in degrees
    public DateTime CreatedDate { get; set; }
}

public class FloorplanElementDetailResponseDto : FloorplanElementResponseDto
{
    public DateTime ModifiedDate { get; set; }
}
public class CombinedTableResponseDto
{
    public Guid Guid { get; set; }
    public string CombinationName { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
    public List<CombinedTableMemberResponseDto> Members { get; set; } = new();
}

public class CombinedTableMemberResponseDto
{
    public Guid Guid { get; set; }
    public string FloorplanElementName { get; set; }
}
