using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Tarabezah.Infrastructure.SignalR
{
    public class TarabezahHub : Hub
    {
        private readonly ILogger<TarabezahHub> _logger;

        public TarabezahHub(ILogger<TarabezahHub> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Restaurant Groups
        public async Task JoinRestaurantGroup(string restaurantGuid)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, restaurantGuid);
            _logger.LogInformation("Client {ConnectionId} joined restaurant group {RestaurantGuid}",
                Context.ConnectionId, restaurantGuid);
        }

        public async Task LeaveRestaurantGroup(string restaurantGuid)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, restaurantGuid);
            _logger.LogInformation("Client {ConnectionId} left restaurant group {RestaurantGuid}",
                Context.ConnectionId, restaurantGuid);
        }
        #endregion

        #region Reservation Groups
        public async Task JoinReservationGroup(string reservationGuid)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"reservation_{reservationGuid}");
            _logger.LogInformation("Client {ConnectionId} joined reservation group {ReservationGuid}",
                Context.ConnectionId, reservationGuid);
        }

        public async Task LeaveReservationGroup(string reservationGuid)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"reservation_{reservationGuid}");
            _logger.LogInformation("Client {ConnectionId} left reservation group {ReservationGuid}",
                Context.ConnectionId, reservationGuid);
        }
        #endregion

        #region Floorplan Groups
        public async Task JoinFloorplanGroup(string floorplanGuid)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"floorplan_{floorplanGuid}");
            _logger.LogInformation("Client {ConnectionId} joined floorplan group {FloorplanGuid}",
                Context.ConnectionId, floorplanGuid);
        }

        public async Task LeaveFloorplanGroup(string floorplanGuid)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"floorplan_{floorplanGuid}");
            _logger.LogInformation("Client {ConnectionId} left floorplan group {FloorplanGuid}",
                Context.ConnectionId, floorplanGuid);
        }
        #endregion

        #region Client Groups
        public async Task JoinClientGroup(string clientGuid)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"client_{clientGuid}");
            _logger.LogInformation("Client {ConnectionId} joined client group {ClientGuid}",
                Context.ConnectionId, clientGuid);
        }

        public async Task LeaveClientGroup(string clientGuid)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"client_{clientGuid}");
            _logger.LogInformation("Client {ConnectionId} left client group {ClientGuid}",
                Context.ConnectionId, clientGuid);
        }
        #endregion

        #region Connection Events
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
        #endregion

        #region Floorplan Events
        public async Task FloorplanPublished()
        {
            await Clients.All.SendAsync("FloorplanPublished");
        }

        public async Task FloorplanCreated()
        {
            await Clients.All.SendAsync("FloorplanCreated");
        }

        public async Task FloorplansUpdated()
        {
            await Clients.All.SendAsync("FloorplansUpdated");
        }

        public async Task FloorplanDataSent()
        {
            await Clients.All.SendAsync("FloorplanDataSent");
        }
        #endregion

        #region Restaurant Events
        public async Task RestaurantCreated()
        {
            await Clients.All.SendAsync("RestaurantCreated");
        }

        public async Task RestaurantCombinedTablesSent()
        {
            await Clients.All.SendAsync("RestaurantCombinedTablesSent");
        }
        #endregion

        #region Reservation Events
        public async Task ReservationCreated()
        {
            await Clients.All.SendAsync("ReservationCreated");
        }

        public async Task ReservationUpdated()
        {
            await Clients.All.SendAsync("ReservationUpdated");
        }

        public async Task ReservationStatusChanged()
        {
            await Clients.All.SendAsync("ReservationStatusChanged");
        }

        public async Task TableAssigned()
        {
            await Clients.All.SendAsync("TableAssigned");
        }

        public async Task TableUnassigned()
        {
            await Clients.All.SendAsync("TableUnassigned");
        }
        #endregion

        #region Client Events
        public async Task ClientCreated()
        {
            await Clients.All.SendAsync("ClientCreated");
        }

        public async Task ClientBlocked()
        {
            await Clients.All.SendAsync("ClientBlocked");
        }
        #endregion

        #region Combined Table Events
        public async Task CombinedTableCreated()
        {
            await Clients.All.SendAsync("CombinedTableCreated");
        }

        public async Task CombinedTableDeleted()
        {
            await Clients.All.SendAsync("CombinedTableDeleted");
        }
        #endregion

        #region Element Events
        public async Task ElementCreated()
        {
            await Clients.All.SendAsync("ElementCreated");
        }

        public async Task ElementUpdated()
        {
            await Clients.All.SendAsync("ElementUpdated");
        }

        public async Task ElementDeleted()
        {
            await Clients.All.SendAsync("ElementDeleted");
        }
        #endregion

        #region Block Table Events
        public async Task TableBlocked()
        {
            await Clients.All.SendAsync("TableBlocked");
        }

        public async Task TableUnblocked()
        {
            await Clients.All.SendAsync("TableUnblocked");
        }
        #endregion

        #region Shift Events
        public async Task ShiftUpdated()
        {
            await Clients.All.SendAsync("ShiftUpdated");
        }
        #endregion

        #region Waitlist Events
        public async Task WaitlistUpdated()
        {
            await Clients.All.SendAsync("WaitlistUpdated");
        }
        #endregion
    }
}