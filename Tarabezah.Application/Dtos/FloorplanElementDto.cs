using System;

namespace Tarabezah.Application.Dtos;

// This DTO is used when creating a floorplan element instance, with data provided from the client
public class FloorplanElementDto
{
    public Guid Guid { get; set; } // Reference to the floorplan element
    public Guid ElementGuid { get; set; } // Reference to the element type (table, chair, etc.)
    public string TableId { get; set; } = string.Empty; // Unique identifier for this table within the floorplan
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public int X { get; set; } // X position on the floorplan
    public int Y { get; set; } // Y position on the floorplan
    public int Width { get; set; } // Width of the element
    public int Height { get; set; } // Height of the element
    public int Rotation { get; set; } // Rotation in degrees
} 