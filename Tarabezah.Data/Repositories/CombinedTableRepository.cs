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
/// Repository implementation for combined table operations
/// </summary>
public class CombinedTableRepository : Repository<CombinedTable>, ICombinedTableRepository
{
    public CombinedTableRepository(TarabezahDbContext context, TimeZoneInfo jordanTimeZone) : base(context, jordanTimeZone)
    {
    }

    public async Task<CombinedTable?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default)
    {
        return await _context.CombinedTables
            .Include(ct => ct.Floorplan)
            .Include(ct => ct.Members)
                .ThenInclude(m => m.FloorplanElementInstance)
                    .ThenInclude(fei => fei.Element)
            .FirstOrDefaultAsync(ct => ct.Guid == guid, cancellationToken);
    }

    public async Task<CombinedTable?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CombinedTables
            .Include(ct => ct.Floorplan)
            .Include(ct => ct.Members)
                .ThenInclude(m => m.FloorplanElementInstance)
                    .ThenInclude(fei => fei.Element)
            .FirstOrDefaultAsync(ct => ct.Id == id, cancellationToken);
    }

    public async Task<List<CombinedTable>> GetByFloorplanIdAsync(int floorplanId, CancellationToken cancellationToken = default)
    {
        return await _context.CombinedTables
            .Include(ct => ct.Members)
                .ThenInclude(m => m.FloorplanElementInstance)
                    .ThenInclude(fei => fei.Element)
            .Where(ct => ct.FloorplanId == floorplanId)
            .OrderBy(ct => ct.GroupName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CombinedTable>> GetByFloorplanGuidAsync(Guid floorplanGuid, CancellationToken cancellationToken = default)
    {
        return await _context.CombinedTables
            .Include(ct => ct.Members)
                .ThenInclude(m => m.FloorplanElementInstance)
                    .ThenInclude(fei => fei.Element)
            .Where(ct => ct.Floorplan.Guid == floorplanGuid)
            .OrderBy(ct => ct.GroupName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CombinedTable combinedTable, CancellationToken cancellationToken = default)
    {
        await _context.CombinedTables.AddAsync(combinedTable, cancellationToken);
    }

    public Task UpdateAsync(CombinedTable combinedTable, CancellationToken cancellationToken = default)
    {
        _context.CombinedTables.Update(combinedTable);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CombinedTable combinedTable, CancellationToken cancellationToken = default)
    {
        _context.CombinedTables.Remove(combinedTable);
        return Task.CompletedTask;
    }
}