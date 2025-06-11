using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Domain.Entities;

namespace Tarabezah.Domain.Repositories;

/// <summary>
/// Repository interface for reservation operations
/// </summary>
public interface IReservationRepository : IRepository<Reservation>
{
    /// <summary>
    /// Gets a reservation by its GUID with all related details
    /// </summary>
    /// <param name="guid">The GUID of the reservation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The reservation with all details if found, or null if not found</returns>
    Task<Reservation?> GetByGuidWithDetailsAsync(Guid guid, CancellationToken cancellationToken = default);
} 