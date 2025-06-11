using System;
using System.Collections.Generic;

namespace Tarabezah.Application.Dtos.Notifications
{
    /// <summary>
    /// DTO for floorplan-related notifications sent via SignalR
    /// </summary>
    public class FloorplanNotificationDto
    {
        /// <summary>
        /// The unique identifier of the floorplan
        /// </summary>
        public Guid FloorplanGuid { get; set; }

        /// <summary>
        /// The name of the floorplan
        /// </summary>
        public string FloorplanName { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the restaurant
        /// </summary>
        public Guid RestaurantGuid { get; set; }

        /// <summary>
        /// The name of the restaurant
        /// </summary>
        public string RestaurantName { get; set; } = string.Empty;

        /// <summary>
        /// Whether the floorplan is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The total capacity of the floorplan
        /// </summary>
        public int TotalCapacity { get; set; }

        /// <summary>
        /// Summary of table counts by type
        /// </summary>
        public List<TableTypeSummary> TableTypeSummaries { get; set; } = new List<TableTypeSummary>();
    }

    /// <summary>
    /// Summary of tables by type
    /// </summary>
    public class TableTypeSummary
    {
        /// <summary>
        /// The type of table
        /// </summary>
        public string TableType { get; set; } = string.Empty;

        /// <summary>
        /// The count of tables of this type
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The total capacity of tables of this type
        /// </summary>
        public int TotalCapacity { get; set; }
    }
}
 
 