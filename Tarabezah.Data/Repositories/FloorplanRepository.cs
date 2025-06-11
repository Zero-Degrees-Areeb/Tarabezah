using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tarabezah.Data.Context;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Data.Repositories;

/// <summary>
/// Repository implementation for floorplan operations
/// </summary>
public class FloorplanRepository : Repository<Floorplan>, IFloorplanRepository
{
    public FloorplanRepository(TarabezahDbContext context, TimeZoneInfo jordanTimeZone)
        : base(context, jordanTimeZone)
    {
    }

    public async Task<Floorplan?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default)
    {
        return await _context.Floorplans
            .Include(f => f.Elements)
                .ThenInclude(e => e.Element)
            .Include(f => f.CombinedTables)
                .ThenInclude(ct => ct.Members)
            .FirstOrDefaultAsync(f => f.Guid == guid, cancellationToken);
    }

    public async Task<Floorplan?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Floorplans
            .Include(f => f.Elements)
                .ThenInclude(e => e.Element)
            .Include(f => f.Restaurant)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<List<Floorplan>> GetByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default)
    {
        return await _context.Floorplans
            .Include(f => f.Elements)
                .ThenInclude(e => e.Element)
            .Where(f => f.RestaurantId == restaurantId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Floorplan>> GetByRestaurantGuidAsync(Guid restaurantGuid, CancellationToken cancellationToken = default)
    {
        return await _context.Floorplans
            .Include(f => f.Elements)
                .ThenInclude(e => e.Element)
            .Include(f => f.Restaurant)
            .Where(f => f.Restaurant.Guid == restaurantGuid)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Floorplan floorplan, CancellationToken cancellationToken = default)
    {
        await _context.Floorplans.AddAsync(floorplan, cancellationToken);
    }

    public Task UpdateAsync(Floorplan floorplan, CancellationToken cancellationToken = default)
    {
        _context.Floorplans.Update(floorplan);
        return Task.CompletedTask;
    }

    public async Task DeleteFloorplanElementsAsync(int floorplanId)
    {
        // First, delete all combined tables for this floorplan
        var combinedTables = await _context.CombinedTables
            .Where(ct => ct.FloorplanId == floorplanId)
            .ToListAsync();

        if (combinedTables.Any())
        {
            _context.CombinedTables.RemoveRange(combinedTables);
            await _context.SaveChangesAsync();
        }

        // Then delete all floorplan elements
        var elements = await _context.FloorplanElements
            .Where(e => e.FloorplanId == floorplanId)
            .ToListAsync();

        if (elements.Any())
        {
            _context.FloorplanElements.RemoveRange(elements);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Floorplan floorplan, CancellationToken cancellationToken = default)
    {
        // First delete all combined tables and elements
        //await DeleteFloorplanElementsAsync(floorplan.Id);

        // Then delete the floorplan itself
        _context.Floorplans.Remove(floorplan);
        await _context.SaveChangesAsync();
    }

    public async Task<Floorplan?> GetFloorplanWithElementsAsync(int id)
    {
        return await _context.Floorplans
            .Include(f => f.Elements)
                .ThenInclude(e => e.Element)
            .Include(f => f.Restaurant)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Floorplan?> GetFloorplanWithElementsByGuidAsync(Guid guid)
    {
        return await _context.Floorplans
            .Include(f => f.Elements)
                .ThenInclude(e => e.Element)
            .Include(f => f.Restaurant)
            .FirstOrDefaultAsync(f => f.Guid == guid);
    }

    public async Task<IEnumerable<Floorplan>> GetFloorplansByRestaurantAsync(int restaurantId)
    {
        return await _context.Floorplans
            .Include(f => f.Elements)
                .ThenInclude(e => e.Element)
            .Where(f => f.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Floorplan>> GetFloorplansByRestaurantGuidAsync(Guid restaurantGuid)
    {
        return await _context.Floorplans
            .Include(f => f.Elements)
                .ThenInclude(e => e.Element)
            .Include(f => f.Restaurant)
            .Where(f => f.Restaurant.Guid == restaurantGuid)
            .ToListAsync();
    }

    public async Task<Floorplan?> GetByNameAndRestaurantGuidAsync(string name, Guid restaurantGuid)
    {
        return await _context.Floorplans
            .Include(f => f.Elements)
                .ThenInclude(e => e.Element)
            .Include(f => f.Restaurant)
            .FirstOrDefaultAsync(f => f.Name == name && f.Restaurant.Guid == restaurantGuid);
    }

    public async Task EnsureElementsLoadedAsync(Floorplan floorplan, CancellationToken cancellationToken = default)
    {
        if (!_context.Entry(floorplan).Collection(f => f.Elements).IsLoaded)
        {
            await _context.Entry(floorplan)
                .Collection(f => f.Elements)
                .Query()
                .Include(e => e.Element)
                .LoadAsync(cancellationToken);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}