using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos.Notifications;
using Tarabezah.Application.Services;
using Tarabezah.Infrastructure.SignalR;

namespace Tarabezah.Web.Services
{
    /// <summary>
    /// SignalR implementation of the INotificationService
    /// </summary>
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<TarabezahHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        /// <summary>
        /// Initializes a new instance of the SignalRNotificationService
        /// </summary>
        public SignalRNotificationService(
            IHubContext<TarabezahHub> hubContext,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Reservation Notifications
        /// <inheritdoc/>
        public async Task NotifyReservationCreatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReservationCreated");
                _logger.LogInformation("Sent ReservationCreated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ReservationCreated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyReservationUpdatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReservationUpdated");
                _logger.LogInformation("Sent ReservationUpdated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ReservationUpdated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyReservationDeletedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReservationDeleted");
                _logger.LogInformation("Sent ReservationDeleted notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ReservationDeleted notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyTableAssignedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TableAssigned");
                _logger.LogInformation("Sent TableAssigned notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending TableAssigned notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyTableUnassignedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TableUnassigned");
                _logger.LogInformation("Sent TableUnassigned notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending TableUnassigned notification");
                throw;
            }
        }
        #endregion

        #region Restaurant Notifications
        /// <inheritdoc/>
        public async Task NotifyRestaurantCreatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("RestaurantCreated");
                _logger.LogInformation("Sent RestaurantCreated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending RestaurantCreated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyRestaurantUpdatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("RestaurantUpdated");
                _logger.LogInformation("Sent RestaurantUpdated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending RestaurantUpdated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyRestaurantDeletedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("RestaurantDeleted");
                _logger.LogInformation("Sent RestaurantDeleted notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending RestaurantDeleted notification");
                throw;
            }
        }
        #endregion

        #region Client Notifications
        /// <inheritdoc/>
        public async Task NotifyClientCreatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ClientCreated");
                _logger.LogInformation("Sent ClientCreated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ClientCreated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyClientUpdatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ClientUpdated");
                _logger.LogInformation("Sent ClientUpdated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ClientUpdated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyClientDeletedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ClientDeleted");
                _logger.LogInformation("Sent ClientDeleted notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ClientDeleted notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyClientBlockedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ClientBlocked");
                _logger.LogInformation("Sent ClientBlocked notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ClientBlocked notification");
                throw;
            }
        }
        #endregion

        #region Floorplan Notifications
        /// <inheritdoc/>
        public async Task NotifyFloorplanCreatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("FloorplanCreated");
                _logger.LogInformation("Sent FloorplanCreated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending FloorplanCreated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyFloorplanUpdatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("FloorplanUpdated");
                _logger.LogInformation("Sent FloorplanUpdated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending FloorplanUpdated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyFloorplanDeletedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("FloorplanDeleted");
                _logger.LogInformation("Sent FloorplanDeleted notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending FloorplanDeleted notification");
                throw;
            }
        }
        #endregion

        #region Table Notifications
        /// <inheritdoc/>
        public async Task NotifyCombinedTableCreatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("CombineTableCreated");
                _logger.LogInformation("Sent CombineTableCreated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending CombineTableCreated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyCombinedTableDeletedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("CombineTableDeleted");
                _logger.LogInformation("Sent CombineTableDeleted notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending CombineTableDeleted notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyTableCreatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TableCreated");
                _logger.LogInformation("Sent TableCreated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending TableCreated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyTableUpdatedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TableUpdated");
                _logger.LogInformation("Sent TableUpdated notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending TableUpdated notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyTableDeletedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TableDeleted");
                _logger.LogInformation("Sent TableDeleted notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending TableDeleted notification");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task NotifyTableStatusChangedAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TableStatusChanged");
                _logger.LogInformation("Sent TableStatusChanged notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending TableStatusChanged notification");
                throw;
            }
        }
        #endregion
    }
}