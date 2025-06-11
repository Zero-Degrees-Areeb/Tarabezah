using System;
using Tarabezah.Domain.Entities;

namespace Tarabezah.Application.Dtos.Notifications
{
    public class ReservationNotificationDto
    {
        public Guid ReservationGuid { get; set; }
        public Guid RestaurantGuid { get; set; }
        public string RestaurantName { get; set; }
        public string ClientName { get; set; }
        public int PartySize { get; set; }
        public DateTime ReservationDate { get; set; }
        public string Time { get; set; }
        public string ShiftName { get; set; }
        public ReservationStatus Status { get; set; }
        public string Notes { get; set; }
        public TableInfoDto TableInfo { get; set; }
    }

    public class TableInfoDto
    {
        public Guid? TableGuid { get; set; }
        public string TableName { get; set; }
        public int? MinCapacity { get; set; }
        public int? MaxCapacity { get; set; }
        public string FloorplanName { get; set; }
    }
}