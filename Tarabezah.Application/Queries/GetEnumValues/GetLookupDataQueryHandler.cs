using MediatR;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Dtos;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Application.Queries.GetEnumValues;

/// <summary>
/// Handler for processing GetLookupDataQuery
/// </summary>
public class GetLookupDataQueryHandler : IRequestHandler<GetLookupDataQuery, LookupDataDto>
{
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly IRepository<RestaurantShift> _restaurantShiftRepository;
    private readonly ILogger<GetLookupDataQueryHandler> _logger;

    // Base URL for icons
    private const string IconBaseUrl = "https://api.iconify.design/fa6";

    // Colors for icons
    private const string WhiteColor = "ffffff";
    private const string GoldColor = "B98858";

    // Icon mappings for ClientSource with style and name
    private static readonly Dictionary<ClientSource, (string Style, string Name)> ClientSourceIcons = new()
    {
        { ClientSource.Website, ("solid", "globe") },
        { ClientSource.Facebook, ("brands", "facebook") },
        { ClientSource.Instagram, ("brands", "instagram") },
        { ClientSource.Google, ("brands", "google") },
        { ClientSource.WalkIn, ("solid", "person-walking") },
        { ClientSource.Referral, ("solid", "user-group") },
        { ClientSource.OpenTable, ("solid", "utensils") },
        { ClientSource.Phone, ("solid", "phone") },
        { ClientSource.Email, ("solid", "envelope") },
        { ClientSource.FriendReferral, ("solid", "users") },
        { ClientSource.Other, ("solid", "circle-question") }
    };

    // Icon mappings for ClientTag with style and name
    private static readonly Dictionary<ClientTag, (string Style, string Name)> ClientTagIcons = new()
    {
        { ClientTag.VIP, ("solid", "crown") },
        { ClientTag.WineLover, ("solid", "wine-glass") },
        { ClientTag.Vegetarian, ("solid", "leaf") },
        { ClientTag.Vegan, ("solid", "seedling") },
        { ClientTag.GlutenFree, ("solid", "bread-slice") },
        { ClientTag.Birthday, ("solid", "cake-candles") },
        { ClientTag.Anniversary, ("solid", "heart") },
        { ClientTag.Regular, ("solid", "star") },
        { ClientTag.HighSpender, ("solid", "dollar-sign") },
        { ClientTag.Allergies, ("solid", "circle-exclamation") },
        { ClientTag.BbqLover, ("solid", "fire") },
        { ClientTag.Pescatarian, ("solid", "fish") },
        { ClientTag.QuietTable, ("solid", "volume-low") },
        { ClientTag.Business, ("solid", "briefcase") }
    };

    public GetLookupDataQueryHandler(
        IRepository<Shift> shiftRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<RestaurantShift> restaurantShiftRepository,
        ILogger<GetLookupDataQueryHandler> logger)
    {
        _shiftRepository = shiftRepository;
        _restaurantRepository = restaurantRepository;
        _restaurantShiftRepository = restaurantShiftRepository;
        _logger = logger;
    }

    public async Task<LookupDataDto> Handle(GetLookupDataQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting lookup data for restaurant with GUID: {RestaurantGuid}", request.RestaurantGuid);

        var result = new LookupDataDto
        {
            // Get client sources with name, value, and icon URL
            ClientSources = GetEnumValues<ClientSource>(ClientSourceIcons),

            // Get client tags with name, value, and icon URL
            ClientTags = GetEnumValues<ClientTag>(ClientTagIcons),

            // Get table types with name and value (no icons)
            TableTypes = GetEnumValues<TableType>(),

            // Get element purposes with name and value (no icons)
            ElementPurposes = GetEnumValues<ElementPurpose>()
        };

        // Get restaurant shifts
        await GetRestaurantShifts(request.RestaurantGuid, result);

        return result;
    }

    /// <summary>
    /// Gets all values from an enum type with both name and integer value
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <returns>List of enum values with name and integer value</returns>
    private List<EnumValueDto> GetEnumValues<T>() where T : Enum
    {
        return GetEnumValues<T>(null);
    }

    /// <summary>
    /// Gets all values from an enum type with name, integer value, and optional icon URLs
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="iconMapping">Optional dictionary mapping enum values to icon style and name</param>
    /// <returns>List of enum values with name, integer value, and icon URLs if provided</returns>
    private List<EnumValueDto> GetEnumValues<T>(Dictionary<T, (string Style, string Name)>? iconMapping) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
        var result = new List<EnumValueDto>();

        foreach (var enumValue in values)
        {
            var dto = new EnumValueDto
            {
                Name = enumValue.ToString(),
                Value = Convert.ToInt32(enumValue)
            };

            if (iconMapping != null && iconMapping.TryGetValue(enumValue, out var icon))
            {
                // Generate URLs for white and gold colored icons with black background
                dto.IconUrlWhite = $"{IconBaseUrl}-{icon.Style}:{icon.Name}.svg?color=%23{WhiteColor}";
                dto.IconUrlGold = $"{IconBaseUrl}-{icon.Style}:{icon.Name}.svg?color=%23{GoldColor}";
            }

            result.Add(dto);
        }

        return result;
    }

    private async Task GetRestaurantShifts(Guid restaurantGuid, LookupDataDto result)
    {
        // Find the restaurant by its GUID
        var restaurant = await _restaurantRepository.GetByGuidAsync(restaurantGuid);

        if (restaurant == null)
        {
            _logger.LogWarning("Restaurant with GUID {RestaurantGuid} not found", restaurantGuid);
            return;
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
                restaurant.Name, restaurantGuid);
            return;
        }

        // Get all shifts and filter by IDs
        var allShifts = await _shiftRepository.GetAllAsync();
        var shifts = allShifts
            .Where(s => shiftIds.Contains(s.Id))
            .OrderBy(s => s.StartTime)
            .ToList();

        _logger.LogInformation("Retrieved {Count} shifts for restaurant {RestaurantName}",
            shifts.Count, restaurant.Name);

        result.RestaurantShifts = shifts.Select(shift => new ShiftDto
        {
            Guid = shift.Guid,
            Name = shift.Name,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            CreatedDate = shift.CreatedDate,
            ModifiedDate = shift.ModifiedDate
        }).ToList();
    }
}