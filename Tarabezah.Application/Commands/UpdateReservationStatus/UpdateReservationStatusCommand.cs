using MediatR;
using System;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;

namespace Tarabezah.Application.Commands.UpdateReservationStatus;

/// <summary>
/// Command to update the status of a reservation
/// </summary>
public class UpdateReservationStatusCommand : IRequest<ReservationDto>
{
    /// <summary>
    /// The GUID of the reservation to update
    /// </summary>
    public Guid ReservationGuid { get; set; }
    
    /// <summary>
    /// The new status for the reservation
    /// </summary>
    public ReservationStatus NewStatus { get; set; }
} 