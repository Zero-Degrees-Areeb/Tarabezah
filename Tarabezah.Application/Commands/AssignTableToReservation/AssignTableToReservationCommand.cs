using MediatR;
using System;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Commands.AssignTableToReservation;

/// <summary>
/// Command to assign a table to a reservation
/// </summary>
public class AssignTableToReservationCommand : IRequest<ReservationDto>
{
    /// <summary>
    /// The GUID of the reservation to assign a table to
    /// </summary>
    public Guid ReservationGuid { get; set; }

    /// <summary>
    /// The GUID of the floorplan element (table) to assign to the reservation.
    /// This is optional if CombinedTableMemberGuid is provided.
    /// </summary>
    public Guid? FloorplanElementGuid { get; set; }

    /// <summary>
    /// The GUID of the combined table member to assign to the reservation.
    /// This is optional if FloorplanElementGuid is provided.
    /// </summary>
    public Guid? CombinedTableMemberGuid { get; set; }
}