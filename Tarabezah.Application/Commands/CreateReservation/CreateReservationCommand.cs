using MediatR;
using System;
using System.Collections.Generic;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;

namespace Tarabezah.Application.Commands.CreateReservation;

/// <summary>
/// Command to create a new reservation
/// </summary>
public class CreateReservationCommand : IRequest<ReservationDto>
{
    /// <summary>
    /// The GUID of the client making the reservation
    /// </summary>
    public Guid ClientGuid { get; set; }

    /// <summary>
    /// The GUID of the restaurant for the reservation
    /// </summary>
    public Guid RestaurantGuid { get; set; }

    /// <summary>
    /// The GUID of the shift for the reservation
    /// </summary>
    public Guid ShiftGuid { get; set; }

    /// <summary>
    /// The date of the reservation
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The time of the reservation within the shift
    /// </summary>
    public TimeSpan? Time { get; set; }

    /// <summary>
    /// The number of people in the party
    /// </summary>
    public int PartySize { get; set; }

    /// <summary>
    /// Tags specific to this reservation (e.g., window seat, high chair)
    /// </summary>
    public List<int>? Tags { get; set; }

    /// <summary>
    /// Special notes or instructions for this reservation
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Optional GUID of the floorplan element to assign to this reservation
    /// </summary>
    public Guid? FloorplanElementGuid { get; set; }

    /// <summary>
    /// boolen for reservation status
    /// </summary>
    public bool IsUpcoming { get; set; }

    /// <summary>
    /// Duration for this reservation in format "HH:mm" (e.g., "01:30" for 1 hour and 30 minutes)
    /// </summary>
    public string? Duration { get; set; }
}