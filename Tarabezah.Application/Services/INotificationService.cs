using System;
using System.Threading.Tasks;
using Tarabezah.Application.Dtos.Notifications;

namespace Tarabezah.Application.Services
{
    /// <summary>
    /// Service for sending real-time notifications to clients
    /// </summary>
    public interface INotificationService
    {
        #region Reservation Notifications
        /// <summary>
        /// Notifies when a reservation is created
        /// </summary>
        Task NotifyReservationCreatedAsync();

        /// <summary>
        /// Notifies when a reservation is updated
        /// </summary>
        Task NotifyReservationUpdatedAsync();

        /// <summary>
        /// Notifies when a reservation is deleted
        /// </summary>
        Task NotifyReservationDeletedAsync();

        /// <summary>
        /// Notifies when a table is assigned to a reservation
        /// </summary>
        Task NotifyTableAssignedAsync();

        /// <summary>
        /// Notifies when a table is unassigned from a reservation
        /// </summary>
        Task NotifyTableUnassignedAsync();
        #endregion

        #region Restaurant Notifications
        /// <summary>
        /// Notifies when a restaurant is created
        /// </summary>
        Task NotifyRestaurantCreatedAsync();

        /// <summary>
        /// Notifies when a restaurant is updated
        /// </summary>
        Task NotifyRestaurantUpdatedAsync();

        /// <summary>
        /// Notifies when a restaurant is deleted
        /// </summary>
        Task NotifyRestaurantDeletedAsync();
        #endregion

        #region Client Notifications
        /// <summary>
        /// Notifies when a client is created
        /// </summary>
        Task NotifyClientCreatedAsync();

        /// <summary>
        /// Notifies when a client is updated
        /// </summary>
        Task NotifyClientUpdatedAsync();

        /// <summary>
        /// Notifies when a client is deleted
        /// </summary>
        Task NotifyClientDeletedAsync();

        /// <summary>
        /// Notifies when a client is blocked
        /// </summary>
        Task NotifyClientBlockedAsync();
        #endregion

        #region Floorplan Notifications
        /// <summary>
        /// Notifies when a floorplan is created
        /// </summary>
        Task NotifyFloorplanCreatedAsync();

        /// <summary>
        /// Notifies when a floorplan is updated
        /// </summary>
        Task NotifyFloorplanUpdatedAsync();

        /// <summary>
        /// Notifies when a floorplan is deleted
        /// </summary>
        Task NotifyFloorplanDeletedAsync();
        #endregion

        #region Table Notifications
        /// <summary>
        /// Notifies when a table combination is created
        /// </summary>
        Task NotifyCombinedTableCreatedAsync();

        /// <summary>
        /// Notifies when a table combination is deleted
        /// </summary>
        Task NotifyCombinedTableDeletedAsync();

        /// <summary>
        /// Notifies when a table is created
        /// </summary>
        Task NotifyTableCreatedAsync();

        /// <summary>
        /// Notifies when a table is updated
        /// </summary>
        Task NotifyTableUpdatedAsync();

        /// <summary>
        /// Notifies when a table is deleted
        /// </summary>
        Task NotifyTableDeletedAsync();

        /// <summary>
        /// Notifies when a table status changes
        /// </summary>
        Task NotifyTableStatusChangedAsync();
        #endregion
    }
}