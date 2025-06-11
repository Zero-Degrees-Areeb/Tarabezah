namespace Tarabezah.Application.Dtos;

/// <summary>
/// DTO for representing reservation status key-value pairs
/// </summary>
public class ReservationStatusDto
{
    /// <summary>
    /// The numeric value of the status
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// The name/description of the status
    /// </summary>
    public string Name { get; set; } = string.Empty;
}