using Microsoft.EntityFrameworkCore;
using Tarabezah.Data.Context;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;
using System;

namespace Tarabezah.Data.Repositories;

public class RestaurantRepository : Repository<Restaurant>, IRestaurantRepository
{
    public RestaurantRepository(TarabezahDbContext context, TimeZoneInfo jordanTimeZone) : base(context, jordanTimeZone)
    {
    }

    public async Task<Restaurant?> GetRestaurantWithFloorplansAsync(int id)
    {
        return await _context.Restaurants
            .Include(r => r.Floorplans)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Restaurant?> GetByGuidAsync(Guid guid)
    {
        return await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Guid == guid);
    }
}