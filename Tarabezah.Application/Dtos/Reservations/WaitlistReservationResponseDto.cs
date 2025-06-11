namespace Tarabezah.Application.Dtos.Reservations;

/// <summary>
/// Response DTO for waitlist reservations with pagination
/// </summary>
public class WaitlistReservationResponseDto
{
    /// <summary>
    /// The waitlist data containing reservations and counts
    /// </summary>
    public WaitlistDataDto Data { get; set; } = new();

    /// <summary>
    /// The current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPrevious { get; set; }

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNext { get; set; }
}

/// <summary>
/// DTO containing waitlist reservation data
/// </summary>
public class WaitlistDataDto
{
    /// <summary>
    /// The status of the reservations (always "Waitlist")
    /// </summary>
    public string ReservationStatus { get; set; } = "Waitlist";

    /// <summary>
    /// The total number of reservations in the waitlist
    /// </summary>
    public int ReservationCount { get; set; }

    /// <summary>
    /// The total number of guests across all waitlist reservations
    /// </summary>
    public int ReservationPartyCount { get; set; }

    /// <summary>
    /// The list of waitlist reservations
    /// </summary>
    public List<WaitlistReservationDetailDto> Reservations { get; set; } = new();
}

/// <summary>
/// DTO containing details of a waitlist reservation
/// </summary>
public class WaitlistReservationDetailDto
{
    /// <summary>
    /// The unique identifier of the reservation
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The name of the client
    /// </summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the client
    /// </summary>
    public Guid? ClientGuid { get; set; }

    /// <summary>
    /// Indicates if the client is blacklisted
    /// </summary>
    public bool IsBlacklisted { get; set; }

    /// <summary>
    /// The tags associated with the client
    /// </summary>
    public List<string> ClientTags { get; set; } = new();

    /// <summary>
    /// The reservation time in HH:mm:ss format
    /// </summary>
    public string Time { get; set; } = string.Empty;

    /// <summary>
    /// The status of the reservation
    /// </summary>
    public string Status { get; set; } = "Waitlist";

    /// <summary>
    /// The size of the party
    /// </summary>
    public int PartySize { get; set; }

    /// <summary>
    /// The type of reservation
    /// </summary>
    public string ReservationType { get; set; } = string.Empty;

    /// <summary>
    /// Any notes associated with the reservation
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// The tags associated with the reservation
    /// </summary>
    public List<string> ReservationTags { get; set; } = new();

    /// <summary>
    /// Information about the assigned tables
    /// </summary>
    public List<TableInformation> TableInfo { get; set; } = new();
}


public class TableInformation
{
    /// <summary>
    /// The unique identifier for the table within the floorplan
    /// </summary>
    public string TableId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the table
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// The GUID of the element
    /// </summary>
    public Guid ElementGuid { get; set; }

    /// <summary>
    /// The minimum seating capacity of the table
    /// </summary>
    public int MinCapacity { get; set; }

    /// <summary>
    /// The maximum seating capacity of the table
    /// </summary>
    public int MaxCapacity { get; set; }

    /// <summary>
    /// The name of the floorplan this table belongs to
    /// </summary>
    public string FloorplanName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this table is part of a combined table
    /// </summary>
    public bool IsCombined { get; set; }

    /// <summary>
    /// The GUID of the combined table this table belongs to (if it's part of a combined table)
    /// </summary>
    public Guid? CombinedTableId { get; set; }
}