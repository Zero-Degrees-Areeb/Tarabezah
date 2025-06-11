using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Data.Context;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Application.Commands.CreateCombinedTable;

public class CreateCombinedTableCommandHandler : IRequestHandler<CreateCombinedTableCommand, CombinedTableDto>
{
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly IFloorplanElementRepository _floorplanElementRepository;
    private readonly ICombinedTableRepository _combinedTableRepository;
    private readonly TarabezahDbContext _dbContext;
    private readonly ILogger<CreateCombinedTableCommandHandler> _logger;

    public CreateCombinedTableCommandHandler(
        IFloorplanRepository floorplanRepository,
        IFloorplanElementRepository floorplanElementRepository,
        ICombinedTableRepository combinedTableRepository,
        TarabezahDbContext dbContext,
        ILogger<CreateCombinedTableCommandHandler> logger)
    {
        _floorplanRepository = floorplanRepository ?? throw new ArgumentNullException(nameof(floorplanRepository));
        _floorplanElementRepository = floorplanElementRepository ?? throw new ArgumentNullException(nameof(floorplanElementRepository));
        _combinedTableRepository = combinedTableRepository ?? throw new ArgumentNullException(nameof(combinedTableRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CombinedTableDto> Handle(CreateCombinedTableCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting the creation of a combined table for floorplan: {FloorplanGuid}", request.FloorplanGuid);

        // Start transaction
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Validate Floorplan
            var floorplan = await _floorplanRepository.GetByGuidAsync(request.FloorplanGuid, cancellationToken);
            if (floorplan == null)
            {
                _logger.LogError("Floorplan not found. Unable to create combined table. Floorplan ID: {FloorplanGuid}", request.FloorplanGuid);
                throw new ArgumentException($"Floorplan with GUID {request.FloorplanGuid} not found");
            }

            // 2. Get and Validate TableInstances
            var floorplanElementInstances = await _floorplanElementRepository.GetByGuidsAsync(
                request.FloorplanElementInstanceGuids,
                cancellationToken);

            // Ensure all requested elements exist
            if (floorplanElementInstances.Count != request.FloorplanElementInstanceGuids.Count)
            {
                var foundGuids = floorplanElementInstances.Select(e => e.Guid);
                var missingGuids = request.FloorplanElementInstanceGuids.Except(foundGuids);
                _logger.LogError("Some elements were not found. Missing element GUIDs: {MissingGuids}", string.Join(", ", missingGuids));
                throw new InvalidOperationException($"Elements with GUIDs {string.Join(", ", missingGuids)} not found");
            }

            // Ensure all elements belong to the same floorplan
            var invalidElements = floorplanElementInstances.Where(e => e.FloorplanId != floorplan.Id).ToList();
            if (invalidElements.Any())
            {
                var invalidGuids = string.Join(", ", invalidElements.Select(e => e.Guid));
                _logger.LogError("Elements do not belong to the same floorplan. Invalid element GUIDs: {InvalidGuids}", invalidGuids);
                throw new InvalidOperationException($"Elements with GUIDs {invalidGuids} don't belong to floorplan {request.FloorplanGuid}");
            }

            // Check if all elements are reservable
            var nonReservableElements = floorplanElementInstances
                .Where(e => e.Element?.Purpose != ElementPurpose.Reservable)
                .ToList();

            if (nonReservableElements.Any())
            {
                var nonReservableGuids = string.Join(", ", nonReservableElements.Select(e => e.Guid));
                _logger.LogError("Some elements are not reservable and cannot be combined. Non-reservable element GUIDs: {NonReservableGuids}", nonReservableGuids);
                throw new InvalidOperationException($"Elements with GUIDs {nonReservableGuids} are decorative and cannot be combined. Only reservable elements can be combined.");
            }

            // Check for duplicate combinations
            var existingCombinedTables = await _combinedTableRepository.GetByFloorplanIdAsync(floorplan.Id, cancellationToken);
            var requestedElementIds = floorplanElementInstances.Select(e => e.Id).OrderBy(id => id).ToList();

            foreach (var existingTable in existingCombinedTables)
            {
                var existingElementIds = existingTable.Members
                    .Select(m => m.FloorplanElementInstanceId)
                    .OrderBy(id => id)
                    .ToList();

                if (requestedElementIds.SequenceEqual(existingElementIds))
                {
                    // Get the table IDs for the requested tables
                    var requestedTableIds = floorplanElementInstances.Select(e => e.TableId).Where(id => !string.IsNullOrEmpty(id));
                    var requestedTableNames = string.Join(", ", requestedTableIds);

                    // Get the table IDs for the existing combined tables
                    var existingTableIds = existingTable.Members
                        .Select(m => m.FloorplanElementInstance.TableId)
                        .Where(id => !string.IsNullOrEmpty(id));
                    var existingTableNames = string.Join(", ", existingTableIds);

                    // Get the group name of the existing combined table
                    var groupName = existingTable.GroupName ?? "Unnamed Group";

                    _logger.LogError(
                        "Cannot combine tables ({RequestedTables}) because they are already combined in group '{GroupName}' with tables ({ExistingTables})",
                        requestedTableNames,
                        groupName,
                        existingTableNames);

                    throw new InvalidOperationException(
                        $"Cannot combine tables ({requestedTableNames}) because they are already combined in group '{groupName}' with tables ({existingTableNames})");
                }
            }

            // 3. Calculate Capacity
            var calculatedMinCapacity = floorplanElementInstances.Sum(e => e.MinCapacity);
            var calculatedMaxCapacity = floorplanElementInstances.Sum(e => e.MaxCapacity);

            // Use provided values if available, otherwise use calculated values
            var minCapacity = request.MinCapacity ?? calculatedMinCapacity;
            var maxCapacity = request.MaxCapacity ?? calculatedMaxCapacity;

            var totalCapacity = (minCapacity + maxCapacity) / 2;

            // 4. Create CombinedTable
            var combinedTable = new CombinedTable
            {
                Guid = Guid.NewGuid(),
                FloorplanId = floorplan.Id,
                GroupName = request.GroupName,
                MinCapacity = minCapacity,
                MaxCapacity = maxCapacity,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            // 5. Create CombinedTableMembers
            foreach (var element in floorplanElementInstances)
            {
                combinedTable.Members.Add(new CombinedTableMember
                {
                    Guid = Guid.NewGuid(),
                    FloorplanElementInstanceId = element.Id,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                });
            }

            // 6. Save Changes
            await _combinedTableRepository.AddAsync(combinedTable, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 7. Commit Transaction
            await transaction.CommitAsync(cancellationToken);

            // 8. Map to DTO
            var dto = new CombinedTableDto
            {
                Guid = combinedTable.Guid,
                GroupName = combinedTable.GroupName,
                MinCapacity = combinedTable.MinCapacity,
                MaxCapacity = combinedTable.MaxCapacity,
                TotalCapacity = totalCapacity,
                Members = combinedTable.Members.Select(m => new CombinedTableMemberDto
                {
                    Guid = m.Guid,
                    FloorplanElementInstanceGuid = m.FloorplanElementInstance.Guid,
                    TableId = m.FloorplanElementInstance.TableId,
                    MinCapacity = m.FloorplanElementInstance.MinCapacity,
                    MaxCapacity = m.FloorplanElementInstance.MaxCapacity
                }).ToList()
            };

            _logger.LogInformation("Successfully created combined table {CombinedTableGuid} with {MemberCount} members", dto.Guid, dto.Members.Count);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating combined table for floorplan {FloorplanGuid}", request.FloorplanGuid);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}