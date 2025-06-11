using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Commands.AssignTableToReservation;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;

public class AssignTableToReservationCommandHandler : IRequestHandler<AssignTableToReservationCommand, ReservationDto>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Element> _elementRepository;
    private readonly IRepository<FloorplanElementInstance> _floorplanElementRepository;
    private readonly IRepository<CombinedTableMember> _combinedTableMemberRepository;
    private readonly IRepository<BlockTable> _blockTableRepository;
    private readonly TimeZoneInfo _jordanTimeZone;
    private readonly ILogger<AssignTableToReservationCommandHandler> _logger;

    public AssignTableToReservationCommandHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<Element> elementRepository,
        IRepository<FloorplanElementInstance> floorplanElementRepository,
        IRepository<CombinedTableMember> combinedTableMemberRepository,
        IRepository<BlockTable> blockTableRepository,
        ILogger<AssignTableToReservationCommandHandler> logger,
        TimeZoneInfo jordanTimeZone)
    {
        _reservationRepository = reservationRepository;
        _elementRepository = elementRepository;
        _floorplanElementRepository = floorplanElementRepository;
        _combinedTableMemberRepository = combinedTableMemberRepository;
        _blockTableRepository = blockTableRepository;
        _logger = logger;
        _jordanTimeZone = jordanTimeZone;
    }

    public async Task<ReservationDto> Handle(AssignTableToReservationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting table assignment process for reservation: {ReservationName}", request.ReservationGuid.ToString());

        // Get Jordan's current time
        var jordanCurrentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _jordanTimeZone);

        // Find the reservation by GUID with included relations
        var reservations = await _reservationRepository.GetAllWithIncludesAsync(new[] { "Client", "Shift" });
        var reservation = reservations.FirstOrDefault(r => r.Guid == request.ReservationGuid);

        if (reservation == null)
        {
            _logger.LogError("Reservation not found. Unable to assign table. Reservation ID: {ReservationGuid}", request.ReservationGuid);
            throw new Exception($"Reservation with GUID {request.ReservationGuid} not found");
        }

        // Check reservation status before assigning table
        if (reservation.Status != ReservationStatus.Upcoming && reservation.Status != null && reservation.Status != ReservationStatus.Seated)
        {
            _logger.LogError("Cannot assign table to reservation {ReservationName} with status {ReservationStatus}", reservation.ReservedElement.TableId ?? request.ReservationGuid.ToString(), reservation.Status);
            throw new Exception($"Cannot assign table to reservation with status {reservation.Status}");
        }

        // Allow table assignment for both present and future reservations
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
                _logger.LogError("The selected table is not reservable. Table Name: {TableId}", floorplanElement.TableId);
                throw new Exception($"Floorplan element with GUID {floorplanElement.TableId} not found");
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
            if (floorplanElement.MaxCapacity < reservation.PartySize)
            {
                _logger.LogError("The selected table does not have enough capacity for the party size. Table ID: {FloorplanElementGuid}, Party size: {PartySize}, Max capacity: {MaxCapacity}", request.FloorplanElementGuid, reservation.PartySize, floorplanElement.MaxCapacity);
                throw new Exception($"Table has insufficient capacity for party of {reservation.PartySize}. Maximum capacity is {floorplanElement.MaxCapacity}.");
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

            // Check for conflicting reservations at the requested time
            var conflictingReservations = existingReservations.Where(r =>
                r.Guid != reservation.Guid && // Don't check against self
                ((r.ReservedElementId == floorplanElement.Id && r.CombinedTableMemberId == null) || // Check single table
                (r.CombinedTableMemberId != null && combinedTableMember != null &&
                 r.CombinedTableMemberId == combinedTableMember.Id)) && // Check combined table
                r.Date.Date == reservation.Date &&
                r.Time < reservation.Time.Add(TimeSpan.FromMinutes(reservation.Duration ?? 0)) &&
                r.Time.Add(TimeSpan.FromMinutes(r.Duration ?? 0)) > reservation.Time &&
                (r.Status == ReservationStatus.Upcoming || r.Status == ReservationStatus.Seated)).ToList();

            if (conflictingReservations.Any())
            {
                var tableId = floorplanElement?.TableId ?? combinedTableMember?.CombinedTable.GroupName ?? "Unknown";
                _logger.LogError("Cannot assign table {TableId} due to existing reservations with status 'upcoming' or 'seated'", tableId);
                throw new Exception($"Cannot assign table {tableId} due to existing reservations with status 'upcoming' or 'seated'.");
            }

            // Assign the table to the reservation
            reservation.ReservedElementId = floorplanElement.Id;
            reservation.CombinedTableMemberId = null;
        }
        // Handle combined table member assignment
        else if (request.CombinedTableMemberGuid.HasValue)
        {
            // Load the CombinedTableMember and its parent CombinedTable
            var combinedTableMembers = await _combinedTableMemberRepository.GetAllWithIncludesAsync(new[] { "CombinedTable", "FloorplanElementInstance" });
            combinedTableMember = combinedTableMembers.FirstOrDefault(ctm => ctm.Guid == request.CombinedTableMemberGuid.Value);

            if (combinedTableMember == null)
            {
                _logger.LogError("Combined table member with GUID {CombinedTableMemberGuid} not found", request.CombinedTableMemberGuid);
                throw new Exception($"Combined table member with GUID {request.CombinedTableMemberGuid} not found");
            }

            // Log the CombinedTableMember
            _logger.LogInformation("CombinedTableMember: {@CombinedTableMember}", combinedTableMember);

            // Ensure the CombinedTable is loaded
            if (combinedTableMember.CombinedTable == null)
            {
                _logger.LogError("Combined table for member {CombinedTableMemberGuid} is not loaded", request.CombinedTableMemberGuid);
                throw new Exception($"Combined table for member {request.CombinedTableMemberGuid} is not loaded");
            }

            // Log the CombinedTable
            _logger.LogInformation("CombinedTable: {@CombinedTable}", combinedTableMember.CombinedTable);

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

            // Check for existing reservations on the combined table
            var existingReservations = await _reservationRepository.GetAllWithIncludesAsync(new[] { "ReservedElement" });

            // Check for conflicting reservations at the requested time
            var conflictingReservations = existingReservations.Where(r =>
                r.Guid != reservation.Guid && // Don't check against self
                r.CombinedTableMemberId == combinedTableMember.Id &&
                r.Date.Date == reservation.Date &&
                r.Time < reservation.Time.Add(TimeSpan.FromMinutes(reservation.Duration ?? 0)) &&
                r.Time.Add(TimeSpan.FromMinutes(r.Duration ?? 0)) > reservation.Time &&
                (r.Status == ReservationStatus.Upcoming || r.Status == ReservationStatus.Seated)).ToList();

            if (conflictingReservations.Any())
            {
                var tableName = combinedTableMember.CombinedTable.GroupName ?? "Unknown";
                _logger.LogError("Cannot assign combined table {TableName} due to existing reservations with status 'upcoming' or 'seated'", tableName);
                throw new Exception($"Cannot assign combined table {tableName} due to existing reservations with status 'upcoming' or 'seated'.");
            }

            // Assign the CombinedTableMember to the reservation
            reservation.CombinedTableMemberId = combinedTableMember.Id;
            reservation.ReservedElementId = null;
        }

        // Update reservation status and save changes
        reservation.ModifiedDate = jordanCurrentTime;

        // Ensure duration is set if not already provided
        if (!reservation.Duration.HasValue)
        {
            reservation.Duration = 300; // Default duration in minutes (5 hours)
        }

        await _reservationRepository.UpdateAsync(reservation);

        // Log success message with appropriate table ID based on the assignment type
        if (floorplanElement != null)
        {
            _logger.LogInformation("Single table successfully assigned to reservation. Reservation: {ReservationGuid}, Table ID: {TableId}",
                request.ReservationGuid.ToString(), floorplanElement.TableId);
        }
        else if (combinedTableMember != null)
        {
            _logger.LogInformation("Combined table successfully assigned to reservation. Reservation: {ReservationGuid}, Combined Table: {CombinedTableName}, Member: {MemberGuid}",
                request.ReservationGuid.ToString(), combinedTableMember.CombinedTable.GroupName, combinedTableMember.Guid);
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

            // Single table assignment details
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
                Height = floorplanElement.Height,
                Width = floorplanElement.Width,
                Rotation = floorplanElement.Rotation,
                CreatedDate = floorplanElement.CreatedDate
            } : null,

            // Combined table assignment details
            CombinedTableGuid = combinedTableMember?.CombinedTable?.Guid,
            CombinedTable = combinedTableMember?.CombinedTable != null ? new CombinedTableResponseDto
            {
                Guid = combinedTableMember.CombinedTable.Guid,
                CombinationName = combinedTableMember.CombinedTable.GroupName ?? "Combined Table",
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