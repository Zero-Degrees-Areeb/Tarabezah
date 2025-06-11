using System;
using System.Collections.Generic;

namespace Tarabezah.Application.Dtos.Notifications
{
    /// <summary>
    /// DTO for client-related notifications sent via SignalR
    /// </summary>
    public class ClientNotificationDto
    {
        /// <summary>
        /// The unique identifier of the client
        /// </summary>
        public Guid ClientGuid { get; set; }

        /// <summary>
        /// The name of the client
        /// </summary>
        public string ClientName { get; set; } = string.Empty;

        /// <summary>
        /// The phone number of the client
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// The email of the client
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The list of tags associated with the client
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// The unique identifier of the restaurant
        /// </summary>
        public Guid RestaurantGuid { get; set; }

        /// <summary>
        /// The name of the restaurant
        /// </summary>
        public string RestaurantName { get; set; } = string.Empty;

        /// <summary>
        /// The reason for the client notification (e.g., Created, Updated, Blocked)
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
}