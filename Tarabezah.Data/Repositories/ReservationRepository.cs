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
/// Repository implementation for reservation operations
/// </summary>
public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(TarabezahDbContext context, TimeZoneInfo jordanTimeZone)
        : base(context, jordanTimeZone)
    {
    }

    public async Task<Reservation?> GetByGuidWithDetailsAsync(Guid guid, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Shift)
            .Include(r => r.ReservedElement)
                .ThenInclude(e => e != null ? e.Element : null)
            .FirstOrDefaultAsync(r => r.Guid == guid, cancellationToken);
    }
}