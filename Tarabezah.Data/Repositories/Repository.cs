using Microsoft.EntityFrameworkCore;
using Tarabezah.Data.Context;
using Tarabezah.Domain.Common;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;

namespace Tarabezah.Data.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly TarabezahDbContext _context;
    protected readonly DbSet<T> _dbSet;
    private readonly TimeZoneInfo _jordanTimeZone;

    public Repository(TarabezahDbContext context, TimeZoneInfo jordanTimeZone)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _jordanTimeZone = jordanTimeZone;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null && entity is BaseEntity baseEntity)
        {
            baseEntity.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(baseEntity.CreatedDate, _jordanTimeZone);
            baseEntity.ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(baseEntity.ModifiedDate, _jordanTimeZone);
        }
        return entity;
    }

    public virtual async Task<T?> GetByGuidAsync(Guid guid)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Guid == guid);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllWithIncludesAsync(string[] includes)
    {
        var query = _dbSet.AsQueryable();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
    public virtual async Task<bool> DeleteDirectAsync(T entity)
    {
        // Check if the entity is a FloorplanElementInstance
        if (entity is FloorplanElementInstance elementInstance)
        {
            // Step 1: Check if the element has active reservations
            var reservations = _context.Reservations
                .Where(x => x.ReservedElementId == elementInstance.Id)
                .ToList();

            foreach (var reserve in reservations)
            {
                reserve.ReservedElementId = null;
            }

            // Step 2: Delete related BlockTable records
            var blockTables = _context.BlockTables
                .Where(b => b.FloorplanElementInstanceId == elementInstance.Id)
                .ToList();

            foreach (var blockTable in blockTables)
            {
                _context.BlockTables.Remove(blockTable);
            }

            // Step 3: Delete related CombinedTableMemberships
            var combinedTableMemberships = _context.CombinedTableMembers
                .Where(m => m.FloorplanElementInstanceId == elementInstance.Id)
                .ToList();

            foreach (var membership in combinedTableMemberships)
            {
                // Check if any reservations are assigned to this CombinedTableMember
                var relatedReservations = _context.Reservations
                    .Where(r => r.CombinedTableMemberId == membership.Id)
                    .ToList();

                // Set CombinedTableMemberId to null for these reservations
                foreach (var reservation in relatedReservations)
                {
                    reservation.CombinedTableMemberId = null;
                }

                // Remove the CombinedTableMember
                _context.CombinedTableMembers.Remove(membership);
            }

            // Step 4: Delete the main entity
            _dbSet.Remove(entity);

            // Save changes
            await _context.SaveChangesAsync();
        }

        // Return true to indicate successful deletion
        return true;
    }

}