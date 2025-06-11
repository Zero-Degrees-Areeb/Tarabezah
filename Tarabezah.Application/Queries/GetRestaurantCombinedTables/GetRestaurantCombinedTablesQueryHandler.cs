using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetRestaurantCombinedTables;

/// <summary>
/// Handler for the GetRestaurantCombinedTablesQuery
/// </summary>
public class GetRestaurantCombinedTablesQueryHandler : IRequestHandler<GetRestaurantCombinedTablesQuery, List<FloorplanCombinedTablesDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IFloorplanRepository _floorplanRepository;
    private readonly ICombinedTableRepository _combinedTableRepository;
    private readonly ILogger<GetRestaurantCombinedTablesQueryHandler> _logger;

    public GetRestaurantCombinedTablesQueryHandler(
        IRestaurantRepository restaurantRepository,
        IFloorplanRepository floorplanRepository,
        ICombinedTableRepository combinedTableRepository,
        ILogger<GetRestaurantCombinedTablesQueryHandler> logger)
    {
        _restaurantRepository = restaurantRepository ?? throw new ArgumentNullException(nameof(restaurantRepository));
        _floorplanRepository = floorplanRepository ?? throw new ArgumentNullException(nameof(floorplanRepository));
        _combinedTableRepository = combinedTableRepository ?? throw new ArgumentNullException(nameof(combinedTableRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<FloorplanCombinedTablesDto>> Handle(GetRestaurantCombinedTablesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting combined tables for restaurant: {RestaurantGuid}", request.RestaurantGuid);

        // Get all floorplans for the restaurant
        var floorplans = await _floorplanRepository.GetByRestaurantGuidAsync(request.RestaurantGuid, cancellationToken);
        if (!floorplans.Any())
        {
            _logger.LogWarning("No floorplans found for restaurant: {RestaurantGuid}", request.RestaurantGuid);
            return new List<FloorplanCombinedTablesDto>();
        }

        var result = new List<FloorplanCombinedTablesDto>();

        // Get combined tables for each floorplan
        foreach (var floorplan in floorplans)
        {
            var combinedTables = await _combinedTableRepository.GetByFloorplanGuidAsync(floorplan.Guid, cancellationToken);

            var combinedTableDtos = combinedTables.Select(ct => new CombinedTableDto
            {
                Guid = ct.Guid,
                GroupName = ct.GroupName,
                MinCapacity = ct.MinCapacity,
                MaxCapacity = ct.MaxCapacity,
                TotalCapacity = ct.MinCapacity.HasValue && ct.MaxCapacity.HasValue
                    ? (ct.MinCapacity.Value + ct.MaxCapacity.Value) / 2
                    : 0,
                Members = ct.Members.Select(m => new CombinedTableMemberDto
                {
                    Guid = m.Guid,
                    FloorplanElementInstanceGuid = m.FloorplanElementInstance.Guid,
                    TableId = m.FloorplanElementInstance.TableId ?? string.Empty,
                    MinCapacity = m.FloorplanElementInstance.MinCapacity,
                    MaxCapacity = m.FloorplanElementInstance.MaxCapacity
                }).ToList()
            }).ToList();

            result.Add(new FloorplanCombinedTablesDto
            {
                FloorplanGuid = floorplan.Guid,
                FloorPlanName = floorplan.Name ?? string.Empty,
                FloorplanCombinedTable = combinedTableDtos
            });
        }

        return result;
    }
}