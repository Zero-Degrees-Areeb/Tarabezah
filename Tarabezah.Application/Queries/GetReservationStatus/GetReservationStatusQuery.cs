using MediatR;
using System.Collections.Generic;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Queries.GetReservationStatus;

/// <summary>
/// Query to get all available reservation statuses
/// </summary>
public class GetReservationStatusQuery : IRequest<List<ReservationStatusDto>>
{
    // No parameters needed as we're just getting all statuses
}