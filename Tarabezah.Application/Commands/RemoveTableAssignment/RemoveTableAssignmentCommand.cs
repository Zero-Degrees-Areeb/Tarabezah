using MediatR;
using System;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Commands.RemoveTableAssignment;

/// <summary>
/// Command to remove a table assignment from a reservation
/// </summary>
public class RemoveTableAssignmentCommand : IRequest<ReservationDto>
{
    /// <summary>
    /// The GUID of the reservation to remove the table assignment from
    /// </summary>
    public Guid ReservationGuid { get; set; }
}