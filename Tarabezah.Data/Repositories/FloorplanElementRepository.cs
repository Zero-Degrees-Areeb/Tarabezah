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
/// Repository implementation for floorplan element operations
/// </summary>
public class FloorplanElementRepository : Repository<FloorplanElementInstance>, IFloorplanElementRepository
{
    public FloorplanElementRepository(TarabezahDbContext context, TimeZoneInfo jordanTimeZone)
        : base(context, jordanTimeZone)
    {
    }

    public async Task<FloorplanElementInstance?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default)
    {
        return await _context.FloorplanElements
            .Include(e => e.Element)
            .Include(e => e.Floorplan)
            .FirstOrDefaultAsync(e => e.Guid == guid, cancellationToken);
    }

    public async Task<List<FloorplanElementInstance>> GetByGuidsAsync(IEnumerable<Guid> guids, CancellationToken cancellationToken = default)
    {
        return await _context.FloorplanElements
            .Include(e => e.Element)
            .Include(e => e.Floorplan)
            .Where(e => guids.Contains(e.Guid))
            .ToListAsync(cancellationToken);
    }

    public async Task<FloorplanElementInstance?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.FloorplanElements
            .Include(e => e.Element)
            .Include(e => e.Floorplan)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<FloorplanElementInstance>> GetByFloorplanIdAsync(int floorplanId, CancellationToken cancellationToken = default)
    {
        return await _context.FloorplanElements
            .Include(e => e.Element)
            .Where(e => e.FloorplanId == floorplanId)
            .OrderBy(e => e.TableId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FloorplanElementInstance>> GetByFloorplanGuidAsync(Guid floorplanGuid, CancellationToken cancellationToken = default)
    {
        return await _context.FloorplanElements
            .Include(e => e.Element)
            .Where(e => e.Floorplan.Guid == floorplanGuid)
            .OrderBy(e => e.TableId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FloorplanElementInstance>> GetByRestaurantGuidAsync(Guid restaurantGuid, CancellationToken cancellationToken = default)
    {
        return await _context.FloorplanElements
            .Include(e => e.Element)
            .Include(e => e.Floorplan)
            .Where(e => e.Floorplan.Restaurant.Guid == restaurantGuid)
            .OrderBy(e => e.TableId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FloorplanElementInstance floorplanElement, CancellationToken cancellationToken = default)
    {
        await _context.FloorplanElements.AddAsync(floorplanElement, cancellationToken);
    }

    public Task UpdateAsync(FloorplanElementInstance floorplanElement, CancellationToken cancellationToken = default)
    {
        _context.FloorplanElements.Update(floorplanElement);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FloorplanElementInstance floorplanElement, CancellationToken cancellationToken = default)
    {
        _context.FloorplanElements.Remove(floorplanElement);
        return Task.CompletedTask;
    }
}