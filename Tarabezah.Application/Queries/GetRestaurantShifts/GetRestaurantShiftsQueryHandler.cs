using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Tarabezah.Application.Queries.GetRestaurantShifts;

/// <summary>
/// Handler for processing GetRestaurantShiftsQuery
/// </summary>
public class GetRestaurantShiftsQueryHandler : IRequestHandler<GetRestaurantShiftsQuery, IEnumerable<ShiftDto>>
{
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly IRepository<RestaurantShift> _restaurantShiftRepository;
    private readonly ILogger<GetRestaurantShiftsQueryHandler> _logger;

    public GetRestaurantShiftsQueryHandler(
        IRepository<Shift> shiftRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<RestaurantShift> restaurantShiftRepository,
        ILogger<GetRestaurantShiftsQueryHandler> logger)
    {
        _shiftRepository = shiftRepository;
        _restaurantRepository = restaurantRepository;
        _restaurantShiftRepository = restaurantShiftRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ShiftDto>> Handle(GetRestaurantShiftsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting shifts for restaurant with GUID: {RestaurantGuid}", request.RestaurantGuid);
        
        // Find the restaurant by its GUID
        var restaurant = await _restaurantRepository.GetByGuidAsync(request.RestaurantGuid);

        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant with GUID {RestaurantGuid} not found", request.RestaurantGuid);
            return Enumerable.Empty<ShiftDto>();
        }

        // Get all restaurant shifts
        var restaurantShifts = await _restaurantShiftRepository.GetAllAsync();
        
        // Filter shifts for this restaurant and get their IDs
        var shiftIds = restaurantShifts
            .Where(rs => rs.RestaurantId == restaurant.Id)
            .Select(rs => rs.ShiftId)
            .ToList();

        if (!shiftIds.Any())
        {
            _logger.LogInformation("No shifts found for restaurant {RestaurantName} (GUID: {RestaurantGuid})", 
                restaurant.Name, request.RestaurantGuid);
            return Enumerable.Empty<ShiftDto>();
        }

        // Get all shifts and filter by IDs
        var allShifts = await _shiftRepository.GetAllAsync();
        var shifts = allShifts
            .Where(s => shiftIds.Contains(s.Id))
            .OrderBy(s => s.StartTime)
            .ToList();

        _logger.LogInformation("Retrieved {Count} shifts for restaurant {RestaurantName}", 
            shifts.Count, restaurant.Name);

        return shifts.Select(shift => new ShiftDto
        {
            Id = shift.Id,
            Guid = shift.Guid,
            Name = shift.Name,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            CreatedDate = shift.CreatedDate,
            ModifiedDate = shift.ModifiedDate
        });
    }
} 