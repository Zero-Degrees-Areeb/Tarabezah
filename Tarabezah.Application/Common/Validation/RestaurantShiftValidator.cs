using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Common.Validation;

/// <summary>
/// Common validation logic for restaurant and shift operations
/// </summary>
public class RestaurantShiftValidator
{
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<RestaurantShift> _restaurantShiftRepository;
    private readonly ILogger _logger;

    public RestaurantShiftValidator(
        IRepository<Restaurant> restaurantRepository,
        IRepository<Shift> shiftRepository,
        IRepository<RestaurantShift> restaurantShiftRepository,
        ILogger logger)
    {
        _restaurantRepository = restaurantRepository;
        _shiftRepository = shiftRepository;
        _restaurantShiftRepository = restaurantShiftRepository;
        _logger = logger;
    }

    public async Task<(Restaurant Restaurant, Shift Shift)> ValidateRestaurantAndShift(Guid restaurantGuid, string shiftName)
    {
        // Verify restaurant exists
        var restaurant = await _restaurantRepository.GetByGuidAsync(restaurantGuid);
        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant with GUID {RestaurantGuid} not found", restaurantGuid);
            throw new ArgumentException($"Restaurant with GUID {restaurantGuid} not found");
        }

        // Get all shifts and find the requested one
        var shifts = await _shiftRepository.GetAllAsync();
        var shift = shifts.FirstOrDefault(s => s.Name.Equals(shiftName, StringComparison.OrdinalIgnoreCase));
        
        if (shift == null)
        {
            _logger.LogWarning("Shift not found: {ShiftName}", shiftName);
            throw new ArgumentException($"Shift not found: {shiftName}");
        }

        // Verify the shift belongs to the restaurant
        var restaurantShifts = await _restaurantShiftRepository.GetAllAsync();
        var isShiftValid = restaurantShifts.Any(rs => 
            rs.RestaurantId == restaurant.Id && 
            rs.ShiftId == shift.Id);

        if (!isShiftValid)
        {
            _logger.LogWarning(
                "Shift {ShiftName} is not associated with restaurant {RestaurantName}",
                shiftName,
                restaurant.Name);
            throw new ArgumentException($"Shift {shiftName} is not associated with restaurant {restaurant.Name}");
        }

        return (restaurant, shift);
    }
} 