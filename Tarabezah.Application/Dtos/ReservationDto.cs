using Tarabezah.Domain.Entities;

namespace Tarabezah.Application.Dtos;

/// <summary>
/// Data transfer object for reservation information
/// </summary>
public class ReservationDto
{
    /// <summary>
    /// The unique identifier for the reservation
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// The unique identifier of the client making the reservation
    /// </summary>
    public Guid? ClientGuid { get; set; }

    /// <summary>
    /// Basic client information
    /// </summary>
    public ClientDto? Client { get; set; }

    /// <summary>
    /// The unique identifier of the shift for the reservation
    /// </summary>
    public Guid ShiftGuid { get; set; }

    /// <summary>
    /// Basic shift information
    /// </summary>
    public ShiftDto? Shift { get; set; }

    /// <summary>
    /// The date of the reservation
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The time of the reservation within the shift
    /// </summary>
    public TimeSpan Time { get; set; }

    /// <summary>
    /// The number of people in the party
    /// </summary>
    public int PartySize { get; set; }

    /// <summary>
    /// The status of the reservation (null when first created)
    /// </summary>
    public ReservationStatus? Status { get; set; }

    /// <summary>
    /// The type of the reservation (OnCall or WalkIn)
    /// </summary>
    public ReservationType Type { get; set; }

    /// <summary>
    /// Tags specific to this reservation (e.g., window seat, high chair)
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Special notes or instructions for this reservation
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the reservation in format "Xh.Ym" (e.g., "1h.30m")
    /// </summary>
    public string Duration { get; set; } = "1h.00m";

    /// <summary>
    /// Date when the reservation was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// The unique identifier of the reserved table (FloorplanElementInstance)
    /// </summary>
    public Guid? ReservedElementGuid { get; set; }

    /// <summary>
    /// Basic information about the reserved table
    /// </summary>
    public FloorplanElementResponseDto? ReservedElement { get; set; }
   
    /// <summary>
    /// The unique identifier of the reserved table (FloorplanElementInstance)
    /// </summary>
    public Guid? CombinedTableGuid { get; set; }
   
    /// <summary>
    /// Basic information about the reserved table
    /// </summary>
    public CombinedTableResponseDto? CombinedTable { get; set; }

    /// <summary>
    /// Basic information about the reserved table
    /// </summary>
    public int? RestaurantGuid { get; set; }
}