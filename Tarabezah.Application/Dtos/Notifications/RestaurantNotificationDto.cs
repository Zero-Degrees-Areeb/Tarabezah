using System;

namespace Tarabezah.Application.Dtos.Notifications
{
    /// <summary>
    /// DTO for restaurant-related notifications sent via SignalR
    /// </summary>
    public class RestaurantNotificationDto
    {
        /// <summary>
        /// The unique identifier of the restaurant
        /// </summary>
        public Guid RestaurantGuid { get; set; }

        /// <summary>
        /// The name of the restaurant
        /// </summary>
        public string RestaurantName { get; set; } = string.Empty;

        /// <summary>
        /// The address of the restaurant
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// The phone number of the restaurant
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// The current status of the restaurant
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}