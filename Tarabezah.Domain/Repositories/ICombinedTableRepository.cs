using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Domain.Entities;

namespace Tarabezah.Domain.Repositories;

/// <summary>
/// Repository interface for combined table operations
/// </summary>
public interface ICombinedTableRepository : IRepository<CombinedTable>
{
    /// <summary>
    /// Gets a combined table by its GUID
    /// </summary>
    /// <param name="guid">The GUID of the combined table</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The combined table if found, or null if not found</returns>
    Task<CombinedTable?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a combined table by its ID
    /// </summary>
    /// <param name="id">The ID of the combined table</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The combined table if found, or null if not found</returns>
    Task<CombinedTable?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all combined tables for a floorplan
    /// </summary>
    /// <param name="floorplanId">The ID of the floorplan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of combined tables</returns>
    Task<List<CombinedTable>> GetByFloorplanIdAsync(int floorplanId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all combined tables for a floorplan by its GUID
    /// </summary>
    /// <param name="floorplanGuid">The GUID of the floorplan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of combined tables</returns>
    Task<List<CombinedTable>> GetByFloorplanGuidAsync(Guid floorplanGuid, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new combined table
    /// </summary>
    /// <param name="combinedTable">The combined table to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task AddAsync(CombinedTable combinedTable, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing combined table
    /// </summary>
    /// <param name="combinedTable">The combined table to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task UpdateAsync(CombinedTable combinedTable, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a combined table
    /// </summary>
    /// <param name="combinedTable">The combined table to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task DeleteAsync(CombinedTable combinedTable, CancellationToken cancellationToken = default);
} 