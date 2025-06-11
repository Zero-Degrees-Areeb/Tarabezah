using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Domain.Entities;

namespace Tarabezah.Domain.Repositories;

/// <summary>
/// Repository interface for floorplan operations
/// </summary>
public interface IFloorplanRepository : IRepository<Floorplan>
{
    /// <summary>
    /// Gets a floorplan by its GUID
    /// </summary>
    /// <param name="guid">The GUID of the floorplan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The floorplan if found, or null if not found</returns>
    Task<Floorplan?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a floorplan by its ID
    /// </summary>
    /// <param name="id">The ID of the floorplan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The floorplan if found, or null if not found</returns>
    Task<Floorplan?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all floorplans for a restaurant
    /// </summary>
    /// <param name="restaurantId">The ID of the restaurant</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of floorplans</returns>
    Task<List<Floorplan>> GetByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all floorplans for a restaurant by its GUID
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of floorplans</returns>
    Task<List<Floorplan>> GetByRestaurantGuidAsync(Guid restaurantGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new floorplan
    /// </summary>
    /// <param name="floorplan">The floorplan to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task AddAsync(Floorplan floorplan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing floorplan
    /// </summary>
    /// <param name="floorplan">The floorplan to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task UpdateAsync(Floorplan floorplan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a floorplan
    /// </summary>
    /// <param name="floorplan">The floorplan to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task DeleteAsync(Floorplan floorplan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a floorplan with its elements
    /// </summary>
    /// <param name="id">The ID of the floorplan</param>
    /// <returns>The floorplan with its elements if found, or null if not found</returns>
    Task<Floorplan?> GetFloorplanWithElementsAsync(int id);

    /// <summary>
    /// Gets a floorplan with its elements by its GUID
    /// </summary>
    /// <param name="guid">The GUID of the floorplan</param>
    /// <returns>The floorplan with its elements if found, or null if not found</returns>
    Task<Floorplan?> GetFloorplanWithElementsByGuidAsync(Guid guid);

    /// <summary>
    /// Gets all floorplans for a restaurant
    /// </summary>
    /// <param name="restaurantId">The ID of the restaurant</param>
    /// <returns>A collection of floorplans</returns>
    Task<IEnumerable<Floorplan>> GetFloorplansByRestaurantAsync(int restaurantId);

    /// <summary>
    /// Gets all floorplans for a restaurant by its GUID
    /// </summary>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <returns>A collection of floorplans</returns>
    Task<IEnumerable<Floorplan>> GetFloorplansByRestaurantGuidAsync(Guid restaurantGuid);

    /// <summary>
    /// Gets a floorplan by its name and restaurant GUID
    /// </summary>
    /// <param name="name">The name of the floorplan</param>
    /// <param name="restaurantGuid">The GUID of the restaurant</param>
    /// <returns>The floorplan if found, or null if not found</returns>
    Task<Floorplan?> GetByNameAndRestaurantGuidAsync(string name, Guid restaurantGuid);

    /// <summary>
    /// Deletes all elements for a specific floorplan
    /// </summary>
    /// <param name="floorplanId">The ID of the floorplan</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task DeleteFloorplanElementsAsync(int floorplanId);

    /// <summary>
    /// Ensures that the floorplan's elements and their related data are loaded
    /// </summary>
    /// <param name="floorplan">The floorplan to load elements for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task EnsureElementsLoadedAsync(Floorplan floorplan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}