using Tarabezah.Domain.Entities;

namespace Tarabezah.Domain.Repositories;

public interface IRestaurantRepository : IRepository<Restaurant>
{
    Task<Restaurant?> GetRestaurantWithFloorplansAsync(int id);
    Task<Restaurant?> GetByGuidAsync(Guid guid);
} 