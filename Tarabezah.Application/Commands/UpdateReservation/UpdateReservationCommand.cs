using MediatR;
using System;
using System.Collections.Generic;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;

namespace Tarabezah.Application.Commands.UpdateReservation;

/// <summary>
/// Command to update an existing reservation
/// </summary>
public class UpdateReservationCommand : IRequest<ReservationDto>
{
    /// <summary>
    /// The GUID of the reservation to update
    /// </summary>
    public Guid ReservationGuid { get; set; }

    /// <summary>
    /// Optional client GUID to update client details
    /// </summary>
    public Guid? ClientGuid { get; set; }

    /// <summary>
    /// Optional client name to update
    /// </summary>
    public string? ClientName { get; set; }

    /// <summary>
    /// Optional client phone number to update
    /// </summary>
    public string? ClientPhone { get; set; }

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
    /// Duration for this reservation in format "HH:mm" (e.g., "01:30" for 1 hour and 30 minutes)
    /// </summary>
    public string Duration { get; set; } = "01:00";
}