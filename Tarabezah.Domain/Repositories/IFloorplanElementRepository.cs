using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Domain.Entities;

namespace Tarabezah.Domain.Repositories;

/// <summary>
/// Repository interface for floorplan element instance operations
/// </summary>
public interface IFloorplanElementRepository : IRepository<FloorplanElementInstance>
{
    /// <summary>
    /// Gets a floorplan element instance by its GUID
    /// </summary>
    /// <param name="guid">The GUID of the floorplan element instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The floorplan element instance if found, or null if not found</returns>
    Task<FloorplanElementInstance?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple floorplan element instances by their GUIDs
    /// </summary>
    /// <param name="guids">The collection of GUIDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of floorplan element instances</returns>
    Task<List<FloorplanElementInstance>> GetByGuidsAsync(IEnumerable<Guid> guids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a floorplan element instance by its ID
    /// </summary>
    /// <param name="id">The ID of the floorplan element instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The floorplan element instance if found, or null if not found</returns>
    Task<FloorplanElementInstance?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all floorplan element instances for a floorplan
    /// </summary>
    /// <param name="floorplanId">The ID of the floorplan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of floorplan element instances</returns>
    Task<List<FloorplanElementInstance>> GetByFloorplanIdAsync(int floorplanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all floorplan element instances for a floorplan by its GUID
    /// </summary>
    /// <param name="floorplanGuid">The GUID of the floorplan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of floorplan element instances</returns>
    Task<List<FloorplanElementInstance>> GetByFloorplanGuidAsync(Guid floorplanGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all floorplan element instances for a floorplan by its restaurant GUID
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of floorplan element instances</returns>
    Task<List<FloorplanElementInstance>> GetByRestaurantGuidAsync(Guid restaurantGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new floorplan element instance
    /// </summary>
    /// <param name="floorplanElement">The floorplan element instance to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task AddAsync(FloorplanElementInstance floorplanElement, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing floorplan element instance
    /// </summary>
    /// <param name="floorplanElement">The floorplan element instance to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task UpdateAsync(FloorplanElementInstance floorplanElement, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a floorplan element instance
    /// </summary>
    /// <param name="floorplanElement">The floorplan element instance to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task DeleteAsync(FloorplanElementInstance floorplanElement, CancellationToken cancellationToken = default);
}