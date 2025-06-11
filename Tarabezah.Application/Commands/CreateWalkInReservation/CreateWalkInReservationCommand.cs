using MediatR;
using System;
using System.Collections.Generic;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Application.Commands.CreateWalkInReservation;

/// <summary>
/// Command to create a new walk-in reservation
/// </summary>
public class CreateWalkInReservationCommand : IRequest<ReservationDto>
{
    /// <summary>
    /// Optional client GUID if the walk-in customer is a registered client
    /// </summary>
    public Guid? ClientGuid { get; set; }

    /// <summary>
    /// The GUID of the restaurant for the reservation
    /// </summary>
    public Guid RestaurantGuid { get; set; }

    /// <summary>
    /// Number of people in the party
    /// </summary>
    public int PartySize { get; set; }

    /// <summary>
    /// List of client tag values from ClientTag enum
    /// </summary>
    public List<int>? TagValues { get; set; }

    /// <summary>
    /// Additional notes for the reservation
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