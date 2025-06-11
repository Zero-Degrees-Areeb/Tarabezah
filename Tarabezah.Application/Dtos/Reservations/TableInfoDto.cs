public class TableInfoDto
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