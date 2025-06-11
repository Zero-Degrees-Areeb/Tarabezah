namespace Tarabezah.Application.Dtos;

/// <summary>
/// Represents an enum value with both its name and numeric value
/// </summary>
public class EnumValueDto
{
    /// <summary>
    /// The name of the enum value
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The integer value of the enum
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// The Font Awesome SVG icon URL for this enum value (if applicable)
    /// </summary>
    public string? IconUrlWhite { get; set; }

    /// <summary>
    /// The Font Awesome SVG icon URL for this enum value (if applicable)
    /// </summary>
    public string? IconUrlGold { get; set; }
} 