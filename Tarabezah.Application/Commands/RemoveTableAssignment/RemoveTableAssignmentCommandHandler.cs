using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.RemoveTableAssignment;

/// <summary>
/// Handler for the RemoveTableAssignmentCommand
/// </summary>
public class RemoveTableAssignmentCommandHandler : IRequestHandler<RemoveTableAssignmentCommand, ReservationDto>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly ILogger<RemoveTableAssignmentCommandHandler> _logger;
    private readonly TimeZoneInfo _jordanTimeZone;

    public RemoveTableAssignmentCommandHandler(
        IRepository<Reservation> reservationRepository,
        ILogger<RemoveTableAssignmentCommandHandler> logger, TimeZoneInfo timeZoneInfo)
    {
        _reservationRepository = reservationRepository;
        _logger = logger;
        _jordanTimeZone = timeZoneInfo;
    }

    public async Task<ReservationDto> Handle(RemoveTableAssignmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing table removal for reservation {ReservationGuid}", request.ReservationGuid);

        // Find the reservation by GUID with included relations
        var reservations = await _reservationRepository.GetAllWithIncludesAsync(
            includes: new[] { "Client", "Shift" });
        var reservation = reservations.FirstOrDefault(r => r.Guid == request.ReservationGuid);

        if (reservation == null)
        {
            _logger.LogError("Reservation with GUID {ReservationGuid} not found", request.ReservationGuid);
            throw new Exception($"Reservation with GUID {request.ReservationGuid} not found");
        }

        // Check if the reservation has a table assigned
        if (reservation.ReservedElementId == null && reservation.CombinedTableMemberId == null)
        {
            _logger.LogError("Reservation {ReservationGuid} does not have a table assigned", request.ReservationGuid);
            throw new Exception($"Reservation {request.ReservationGuid} does not have a table assigned.");
        }

        // Remove the table assignment
        reservation.ReservedElementId = null;
        reservation.CombinedTableMemberId = null;
        reservation.ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _jordanTimeZone);

        // Update reservation and save changes
        await _reservationRepository.UpdateAsync(reservation);

        _logger.LogInformation("Successfully removed table assignment for reservation {ReservationGuid}", request.ReservationGuid);

        // Return the updated reservation DTO
        return new ReservationDto
        {
            Guid = reservation.Guid,
            ClientGuid = reservation.Client?.Guid,
            Client = reservation.Client != null ? new ClientDto
            {
                Guid = reservation.Client.Guid,
                Name = reservation.Client.Name,
                PhoneNumber = reservation.Client.PhoneNumber,
                Email = reservation.Client.Email,
                CreatedDate = reservation.Client.CreatedDate
            } : null,
            ShiftGuid = reservation.Shift.Guid,
            Shift = new ShiftDto
            {
                Guid = reservation.Shift.Guid,
                Name = reservation.Shift.Name,
                StartTime = reservation.Shift.StartTime,
                EndTime = reservation.Shift.EndTime
            },
            Date = reservation.Date,
            Time = reservation.Time,
            PartySize = reservation.PartySize,
            Status = reservation.Status,
            Type = reservation.Type,
            Tags = reservation.Tags,
            Notes = reservation.Notes,
            CreatedDate = reservation.CreatedDate,
            ReservedElementGuid = null,
            ReservedElement = null
        };
    }
}