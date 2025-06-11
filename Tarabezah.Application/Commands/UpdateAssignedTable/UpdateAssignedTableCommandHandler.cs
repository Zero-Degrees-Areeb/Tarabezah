using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Commands.UpdateAssignedTable;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;

public class UpdateAssignedTableCommandHandler : IRequestHandler<UpdateAssignedTableCommand, ReservationDto>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Element> _elementRepository;
    private readonly IRepository<FloorplanElementInstance> _floorplanElementRepository;
    private readonly IRepository<CombinedTableMember> _combinedTableMemberRepository;
    private readonly IRepository<BlockTable> _blockTableRepository;
    private readonly TimeZoneInfo _jordanTimeZone;
    private readonly ILogger<UpdateAssignedTableCommandHandler> _logger;

    public UpdateAssignedTableCommandHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<Element> elementRepository,
        IRepository<FloorplanElementInstance> floorplanElementRepository,
        IRepository<CombinedTableMember> combinedTableMemberRepository,
        IRepository<BlockTable> blockTableRepository,
        TimeZoneInfo jordanTimeZone,
        ILogger<UpdateAssignedTableCommandHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _elementRepository = elementRepository;
        _floorplanElementRepository = floorplanElementRepository;
        _combinedTableMemberRepository = combinedTableMemberRepository;
        _blockTableRepository = blockTableRepository;
        _jordanTimeZone = jordanTimeZone;
        _logger = logger;
    }

    public async Task<ReservationDto> Handle(UpdateAssignedTableCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting table update process for reservation: {ReservationName}", request.ReservationGuid.ToString());

        // Get Jordan's current time
        var jordanCurrentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _jordanTimeZone);

        // Find the reservation by GUID with included relations
        var reservations = await _reservationRepository.GetAllWithIncludesAsync(new[] { "Client", "Shift", "ReservedElement" });
        var reservation = reservations.FirstOrDefault(r => r.Guid == request.ReservationGuid);

        if (reservation == null)
        {
            _logger.LogError("Reservation not found. Unable to update table. Reservation ID: {ReservationGuid}", request.ReservationGuid);
            throw new Exception($"Reservation with GUID {request.ReservationGuid} not found");
        }

        // Check if reservation has a conflicting table assignment
        if (reservation.ReservedElementId.HasValue || reservation.CombinedTableMemberId.HasValue)
        {
            // Get all reservations for the same table/combined table
            var existingReservations = await _reservationRepository.GetAllWithIncludesAsync(new[] { "ReservedElement" });
            var conflictingReservations = existingReservations.Where(r =>
                r.Guid != reservation.Guid && // Don't check against self
                ((r.ReservedElementId.HasValue && r.ReservedElementId == reservation.ReservedElementId) || // Same single table
                (r.CombinedTableMemberId.HasValue && r.CombinedTableMemberId == reservation.CombinedTableMemberId)) && // Same combined table
                r.Date.Date == reservation.Date &&
                r.Time < reservation.Time.Add(TimeSpan.FromMinutes(reservation.Duration ?? 0)) &&
                r.Time.Add(TimeSpan.FromMinutes(r.Duration ?? 0)) > reservation.Time &&
                (r.Status == ReservationStatus.Upcoming || r.Status == ReservationStatus.Seated)).ToList();

            if (conflictingReservations.Any())
            {
                var tableType = reservation.ReservedElementId.HasValue ? "single" : "combined";
                var tableId = reservation.ReservedElementId.HasValue ?
                    (reservation.ReservedElement?.TableId ?? "Unknown") :
                    "Combined Table";

                _logger.LogError(
                    "Cannot update table - existing {TableType} table assignment ({TableId}) has conflicting reservations",
                    tableType, tableId);
                throw new Exception($"Cannot update table because the current {tableType} table ({tableId}) has conflicting reservations during this time period.");
            }
            else
            {
                _logger.LogInformation(
                    "Existing table assignment found but no conflicts detected. Proceeding with update for reservation {ReservationGuid}",
                    reservation.Guid);
            }
        }

        if (reservation.Status != ReservationStatus.Upcoming && reservation.Status != null && reservation.Status != ReservationStatus.Seated)
        {
            var reservationIdentifier = reservation.Client?.Name ?? request.ReservationGuid.ToString();
            _logger.LogError("Cannot update table for reservation {ReservationName} with status {ReservationStatus}",
                reservationIdentifier, reservation.Status);
            throw new Exception($"Cannot update table for reservation with status {reservation.Status}");
        }

        var reservationStart = reservation.Date.Add(reservation.Time);
        var durationMinutes = reservation.Duration ?? 0;
        var reservationEnd = reservationStart.AddMinutes(durationMinutes);

        // Remove the time check completely since we want to allow table assignment for future reservations
        // This allows assigning tables to reservations regardless of their time window

        // Validate shift availability based on Jordan time
        var shiftStartJordan = reservation.Date.Date + reservation.Shift.StartTime;
        var shiftEndJordan = reservation.Date.Date + reservation.Shift.EndTime;

        if (reservation.Time < reservation.Shift.StartTime || reservation.Time > reservation.Shift.EndTime)
        {
            _logger.LogError("Reservation time is outside the allowed shift time range. Reservation time: {Time}, Shift start: {StartTime}, Shift end: {EndTime}",
                FormatTimeToAmPm(reservation.Time),
                FormatTimeToAmPm(reservation.Shift.StartTime),
                FormatTimeToAmPm(reservation.Shift.EndTime));
            throw new Exception($"Reservation time {FormatTimeToAmPm(reservation.Time)} is outside the shift time range {FormatTimeToAmPm(reservation.Shift.StartTime)} - {FormatTimeToAmPm(reservation.Shift.EndTime)}");
        }

        // Validate that exactly one assignment type is provided
        if (request.FloorplanElementGuid.HasValue && request.CombinedTableMemberGuid.HasValue)
        {
            throw new Exception("Cannot assign both a regular table and a combined table member. Please choose one.");
        }

        if (!request.FloorplanElementGuid.HasValue && !request.CombinedTableMemberGuid.HasValue)
        {
            throw new Exception("Either FloorplanElementGuid or CombinedTableMemberGuid must be provided.");
        }

        FloorplanElementInstance? floorplanElement = null;
        CombinedTableMember? combinedTableMember = null;

        // Handle regular table assignment
        if (request.FloorplanElementGuid.HasValue)
        {
            floorplanElement = await _floorplanElementRepository.GetByGuidAsync(request.FloorplanElementGuid.Value);
            if (floorplanElement == null)
            {
                _logger.LogError("Floorplan element with GUID {FloorplanElementGuid} not found", request.FloorplanElementGuid);
                throw new Exception($"Floorplan element with GUID {request.FloorplanElementGuid} not found");
            }

            // Find Elements 
            var elements = await _elementRepository.GetAllAsync();
            var element = elements.FirstOrDefault(e => e.Id == floorplanElement.ElementId);

            // Check if the element is a reservable table
            if (floorplanElement.ElementId == null || element.Purpose != ElementPurpose.Reservable)
            {
                _logger.LogError("The selected table is not reservable. Table Name: {TableId}", floorplanElement.TableId);
                throw new Exception($"Floorplan element {floorplanElement.TableId} is not a reservable table");
            }

            // Check if the table has sufficient capacity for the party size
            if (floorplanElement.MaxCapacity < reservation.PartySize || floorplanElement.MinCapacity > reservation.PartySize)
            {
                _logger.LogError(
                    "Party size {PartySize} is not within the allowed range for table {TableId}. Min: {Min}, Max: {Max}",
                    reservation.PartySize, floorplanElement.TableId, floorplanElement.MinCapacity, floorplanElement.MaxCapacity);
                throw new Exception(
                    $"Party size {reservation.PartySize} is not within the allowed range for table {floorplanElement.TableId} (Min: {floorplanElement.MinCapacity}, Max: {floorplanElement.MaxCapacity}).");
            }

            // Check if the table is blocked during the reservation time
            var blockedTables = await _blockTableRepository.GetAllWithIncludesAsync(Array.Empty<string>());
            var isBlocked = blockedTables.Any(bt =>
                bt.FloorplanElementInstanceId == floorplanElement.Id &&
                TimeZoneInfo.ConvertTimeFromUtc(bt.StartDate, _jordanTimeZone).Date <= reservation.Date &&
                TimeZoneInfo.ConvertTimeFromUtc(bt.EndDate, _jordanTimeZone).Date >= reservation.Date &&
                bt.StartTime < reservation.Time.Add(TimeSpan.FromMinutes(reservation.Duration ?? 0)) &&
                bt.EndTime > reservation.Time);

            if (isBlocked)
            {
                _logger.LogError("The selected table is blocked during the requested reservation time. Table Name: {TableId}", floorplanElement.TableId);
                throw new Exception($"Table {floorplanElement.TableId} is blocked during the reservation time.");
            }

            // Check for existing reservations
            var existingReservations = await _reservationRepository.GetAllWithIncludesAsync(new[] { "ReservedElement" });

            // Check for conflicting single table reservations
            var conflictingSingleTableReservations = existingReservations.Where(r =>
                r.Guid != reservation.Guid && // Don't check against self
                r.ReservedElementId == floorplanElement.Id && // Check single table assignments
                r.CombinedTableMemberId == null && // Make sure it's not part of a combined table
                r.Date.Date == reservation.Date &&
                r.Time < reservation.Time.Add(TimeSpan.FromMinutes(reservation.Duration ?? 0)) &&
                r.Time.Add(TimeSpan.FromMinutes(r.Duration ?? 0)) > reservation.Time &&
                (r.Status == ReservationStatus.Upcoming || r.Status == ReservationStatus.Seated)).ToList();

            if (conflictingSingleTableReservations.Any())
            {
                _logger.LogError("Cannot update table {TableId} - conflicting reservations found", floorplanElement.TableId);
                throw new Exception($"Table {floorplanElement.TableId} is already reserved during this time period.");
            }

            // Check if this table is part of any combined table reservations
            var conflictingCombinedReservations = existingReservations.Where(r =>
                r.Guid != reservation.Guid &&
                r.CombinedTableMemberId.HasValue &&
                r.Date.Date == reservation.Date &&
                r.Time < reservation.Time.Add(TimeSpan.FromMinutes(reservation.Duration ?? 0)) &&
                r.Time.Add(TimeSpan.FromMinutes(r.Duration ?? 0)) > reservation.Time &&
                (r.Status == ReservationStatus.Upcoming || r.Status == ReservationStatus.Seated)).ToList();

            if (conflictingCombinedReservations.Any())
            {
                _logger.LogError("Cannot update table {TableId} - table is part of a combined table reservation", floorplanElement.TableId);
                throw new Exception($"Table {floorplanElement.TableId} is part of a combined table reservation during this time period.");
            }

            // Update the table assignment - using the correct ID
            reservation.ReservedElementId = floorplanElement.Id; // Use the actual ID, not GUID
            reservation.CombinedTableMemberId = null;

            _logger.LogInformation(
                "Successfully updated single table. ReservationId: {ReservationId}, TableId: {TableId}, ElementId: {ElementId}",
                reservation.Id,
                floorplanElement.TableId,
                floorplanElement.Id);
        }
        // Handle combined table member assignment
        else if (request.CombinedTableMemberGuid.HasValue)
        {
            // Load the CombinedTableMember and its parent CombinedTable
            var combinedTableMembers = await _combinedTableMemberRepository.GetAllWithIncludesAsync(new[] { "CombinedTable", "CombinedTable.Members", "FloorplanElementInstance" });
            combinedTableMember = combinedTableMembers.FirstOrDefault(ctm => ctm.Guid == request.CombinedTableMemberGuid.Value);

            if (combinedTableMember == null)
            {
                _logger.LogError("Combined table member with GUID {CombinedTableMemberGuid} not found", request.CombinedTableMemberGuid);
                throw new Exception($"Combined table member with GUID {request.CombinedTableMemberGuid} not found");
            }

            // Ensure the CombinedTable is loaded
            if (combinedTableMember.CombinedTable == null)
            {
                _logger.LogError("Combined table for member {CombinedTableMemberGuid} is not loaded", request.CombinedTableMemberGuid);
                throw new Exception($"Combined table for member {request.CombinedTableMemberGuid} is not loaded");
            }

            // Get all members of this combination
            var allCombinationMembers = combinedTableMember.CombinedTable.Members;

            // Validate the CombinedTable's capacity
            var combinedTable = combinedTableMember.CombinedTable;

            // Validate party size against the combination's min and max capacity
            if ((combinedTable.MinCapacity.HasValue && reservation.PartySize < combinedTable.MinCapacity.Value) ||
                (combinedTable.MaxCapacity.HasValue && reservation.PartySize > combinedTable.MaxCapacity.Value))
            {
                _logger.LogError(
                    "Party size {PartySize} is not within the allowed range for the combined table. Min: {Min}, Max: {Max}",
                    reservation.PartySize, combinedTable.MinCapacity, combinedTable.MaxCapacity);
                throw new Exception(
                    $"Party size {reservation.PartySize} is not within the allowed range for the combined table (Min: {combinedTable.MinCapacity}, Max: {combinedTable.MaxCapacity}).");
            }

            // Check for blocked tables in the combination
            var blockedTables = await _blockTableRepository.GetAllWithIncludesAsync(Array.Empty<string>());
            var blockedTableInCombination = allCombinationMembers
                .Where(m => m.FloorplanElementInstance != null)
                .Any(m => blockedTables.Any(bt =>
                    bt.FloorplanElementInstanceId == m.FloorplanElementInstance.Id &&
                    TimeZoneInfo.ConvertTimeFromUtc(bt.StartDate, _jordanTimeZone).Date <= reservation.Date &&
                    TimeZoneInfo.ConvertTimeFromUtc(bt.EndDate, _jordanTimeZone).Date >= reservation.Date &&
                    bt.StartTime < reservation.Time.Add(TimeSpan.FromMinutes(reservation.Duration ?? 0)) &&
                    bt.EndTime > reservation.Time));

            if (blockedTableInCombination)
            {
                _logger.LogError("One or more tables in the combination are blocked during the requested time");
                throw new Exception("One or more tables in the combination are blocked during the requested time");
            }

            // Check for existing reservations that might conflict
            var existingReservations = await _reservationRepository.GetAllWithIncludesAsync(
                new[] { "ReservedElement", "CombinedTableMember" });

            // Check for any conflicting reservations in the same combination
            var conflictingReservations = existingReservations.Where(r =>
                r.Guid != reservation.Guid && // Don't check against self
                ((r.CombinedTableMemberId.HasValue &&
                  allCombinationMembers.Any(m => m.Id == r.CombinedTableMemberId)) || // Check combined table conflicts
                 (r.ReservedElementId.HasValue &&
                  allCombinationMembers.Any(m => m.FloorplanElementInstance?.Id == r.ReservedElementId))) && // Check single table conflicts
                r.Date.Date == reservation.Date &&
                r.Time < reservation.Time.Add(TimeSpan.FromMinutes(reservation.Duration ?? 0)) &&
                r.Time.Add(TimeSpan.FromMinutes(r.Duration ?? 0)) > reservation.Time &&
                (r.Status == ReservationStatus.Upcoming || r.Status == ReservationStatus.Seated)).ToList();

            if (conflictingReservations.Any())
            {
                _logger.LogError(
                    "Cannot update combination {CombinationId} - conflicting reservations exist",
                    combinedTableMember.CombinedTableId);
                throw new Exception("One or more tables in this combination are already reserved during this time period.");
            }

            // Update the IDs for the reservation
            reservation.CombinedTableMemberId = combinedTableMember.Id;
            reservation.ReservedElementId = null;

            _logger.LogInformation(
                "Successfully updated combined table assignment. ReservationId: {ReservationId}, CombinedTableId: {CombinedTableId}, MemberId: {MemberId}",
                reservation.Id,
                combinedTableMember.CombinedTableId,
                combinedTableMember.Id);
        }

        // Update reservation status and save changes
        reservation.ModifiedDate = jordanCurrentTime;

        // Ensure duration is set if not already provided
        if (!reservation.Duration.HasValue)
        {
            reservation.Duration = 300; // Default duration in minutes (5 hours)
        }

        try
        {
            await _reservationRepository.UpdateAsync(reservation);
            _logger.LogInformation(
                "Successfully updated reservation in database. ReservationId: {ReservationId}, AssignedId: {AssignedId}",
                reservation.Id,
                reservation.CombinedTableMemberId ?? reservation.ReservedElementId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to update reservation. ReservationId: {ReservationId}, AssignedId: {AssignedId}",
                reservation.Id,
                reservation.CombinedTableMemberId ?? reservation.ReservedElementId);
            throw;
        }

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
            ReservedElementGuid = floorplanElement?.Guid,
            ReservedElement = floorplanElement != null ? new FloorplanElementResponseDto
            {
                Guid = floorplanElement.Guid,
                TableId = floorplanElement.TableId,
                ElementGuid = floorplanElement.Element.Guid,
                ElementName = floorplanElement.Element.Name,
                ElementImageUrl = floorplanElement.Element.ImageUrl,
                ElementType = floorplanElement.Element.TableType.ToString(),
                MinCapacity = floorplanElement.MinCapacity,
                MaxCapacity = floorplanElement.MaxCapacity,
                X = floorplanElement.X,
                Y = floorplanElement.Y,
                Width = floorplanElement.Width,
                Height = floorplanElement.Height,
                Rotation = floorplanElement.Rotation,
                CreatedDate = floorplanElement.CreatedDate
            } : null,
            CombinedTableGuid = combinedTableMember?.CombinedTable?.Guid,
            CombinedTable = combinedTableMember?.CombinedTable != null ? new CombinedTableResponseDto
            {
                Guid = combinedTableMember.CombinedTable.Guid,
                CombinationName = combinedTableMember.CombinedTable.GroupName,
                MinCapacity = combinedTableMember.CombinedTable.MinCapacity,
                MaxCapacity = combinedTableMember.CombinedTable.MaxCapacity,
                Members = combinedTableMember.CombinedTable.Members.Select(m => new CombinedTableMemberResponseDto
                {
                    Guid = m.Guid,
                    FloorplanElementName = m.FloorplanElementInstance?.TableId
                }).ToList()
            } : null
        };
    }

    private string FormatTimeToAmPm(TimeSpan time)
    {
        return DateTime.Today.Add(time).ToString("h:mm tt");
    }
}