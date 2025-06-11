using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Commands.UpdateReservationStatus;

/// <summary>
/// Handler for the UpdateReservationStatusCommand
/// </summary>
public class UpdateReservationStatusCommandHandler : IRequestHandler<UpdateReservationStatusCommand, ReservationDto>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ILogger<UpdateReservationStatusCommandHandler> _logger;

    public UpdateReservationStatusCommandHandler(
        IReservationRepository reservationRepository,
        ILogger<UpdateReservationStatusCommandHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _logger = logger;
    }

    public async Task<ReservationDto> Handle(UpdateReservationStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating reservation {ReservationGuid} status to {NewStatus}",
            request.ReservationGuid, request.NewStatus);

        // Find the reservation by GUID with all related entities
        var reservation = await _reservationRepository.GetByGuidWithDetailsAsync(request.ReservationGuid, cancellationToken);
        if (reservation == null)
        {
            _logger.LogError("Reservation with GUID {ReservationGuid} not found", request.ReservationGuid);
            throw new Exception($"Reservation with GUID {request.ReservationGuid} not found");
        }

        // Update the status
        reservation.Status = request.NewStatus;
        reservation.ModifiedDate = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation);

        _logger.LogInformation("Successfully updated reservation {ReservationGuid} status to {NewStatus}",
            request.ReservationGuid, request.NewStatus);

        // Return the updated reservation DTO with null checks for all navigation properties
        return new ReservationDto
        {
            Guid = reservation.Guid,
            ClientGuid = reservation.Client?.Guid ?? Guid.Empty,
            Client = reservation.Client != null ? new ClientDto
            {
                Guid = reservation.Client.Guid,
                Name = reservation.Client.Name,
                PhoneNumber = reservation.Client.PhoneNumber,
                Email = reservation.Client.Email,
                CreatedDate = reservation.Client.CreatedDate
            } : null,
            ShiftGuid = reservation.Shift?.Guid ?? Guid.Empty,
            Shift = reservation.Shift != null ? new ShiftDto
            {
                Guid = reservation.Shift.Guid,
                Name = reservation.Shift.Name,
                StartTime = reservation.Shift.StartTime,
                EndTime = reservation.Shift.EndTime
            } : null,
            Date = reservation.Date,
            Time = reservation.Time,
            PartySize = reservation.PartySize,
            Status = reservation.Status,
            Type = reservation.Type,
            Tags = reservation.Tags,
            Notes = reservation.Notes ?? string.Empty,
            CreatedDate = reservation.CreatedDate,
            ReservedElementGuid = reservation.ReservedElement?.Guid,
            ReservedElement = reservation.ReservedElement != null ? new FloorplanElementResponseDto
            {
                Guid = reservation.ReservedElement.Guid,
                TableId = reservation.ReservedElement.TableId,
                ElementGuid = reservation.ReservedElement.Element?.Guid ?? Guid.Empty,
                ElementName = reservation.ReservedElement.Element?.Name ?? string.Empty,
                ElementImageUrl = reservation.ReservedElement.Element?.ImageUrl,
                ElementType = reservation.ReservedElement.Element?.TableType.ToString() ?? string.Empty,
                MinCapacity = reservation.ReservedElement.MinCapacity,
                MaxCapacity = reservation.ReservedElement.MaxCapacity,
                X = reservation.ReservedElement.X,
                Y = reservation.ReservedElement.Y,
                Height = reservation.ReservedElement.Height,
                Width = reservation.ReservedElement.Width,
                Rotation = reservation.ReservedElement.Rotation,
                CreatedDate = reservation.ReservedElement.CreatedDate
            } : null
        };
    }
}