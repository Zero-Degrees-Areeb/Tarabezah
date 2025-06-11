using System;

namespace Tarabezah.Application.Dtos.Notifications
{
    /// <summary>
    /// DTO for table-related notifications sent via SignalR
    /// </summary>
    public class TableNotificationDto
    {
        /// <summary>
        /// The unique identifier of the table
        /// </summary>
        public Guid TableGuid { get; set; }

        /// <summary>
        /// The name of the table
        /// </summary>
        public string TableName { get; set; } = string.Empty;

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
        /// The type of the table
        /// </summary>
        public string TableType { get; set; } = string.Empty;

        /// <summary>
        /// The minimum capacity of the table
        /// </summary>
        public int MinCapacity { get; set; }

        /// <summary>
        /// The maximum capacity of the table
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Indicates if this is a combined table
        /// </summary>
        public bool IsCombined { get; set; }

        /// <summary>
        /// True if the table is currently assigned to a reservation
        /// </summary>
        public bool IsReserved { get; set; }

        /// <summary>
        /// The status of the table (Available, Reserved, Occupied, etc.)
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}