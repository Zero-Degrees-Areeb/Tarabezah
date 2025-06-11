using Microsoft.EntityFrameworkCore;
using Tarabezah.Data.Context;
using Tarabezah.Data.Repositories;
using Tarabezah.Domain.Entities;

namespace Tarabezah.Tests.Repositories;

public class FloorplanRepositoryTests
{
    private readonly DbContextOptions<TarabezahDbContext> _options;

    public FloorplanRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<TarabezahDbContext>()
            .UseInMemoryDatabase(databaseName: "TarabezahTestDb_" + Guid.NewGuid())
            .Options;
    }

    [Fact]
    public async Task GetFloorplanWithElementsAsync_ReturnsFloorplanWithElements()
    {
        // Arrange
        var elementId = 1;
        var element = new Element
        {
            Id = elementId,
            Name = "Test Element",
            CreatedDate = DateTime.UtcNow
        };

        var testFloorplan = new Floorplan
        {
            Name = "Test Floorplan",
            CreatedDate = DateTime.UtcNow,
            Elements = new List<FloorplanElementInstance>
            {
                new() { TableId = "Table 1", MinCapacity = 2, MaxCapacity = 4, X = 10, Y = 20, Rotation = 0, ElementId = elementId },
                new() { TableId = "Table 2", MinCapacity = 2, MaxCapacity = 2, X = 30, Y = 40, Rotation = 0, ElementId = elementId }
            }
        };

        using (var context = new TarabezahDbContext(_options))
        {
            context.Elements.Add(element);
            await context.SaveChangesAsync();
            
            context.Floorplans.Add(testFloorplan);
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new TarabezahDbContext(_options))
        {
            var repository = new FloorplanRepository(context, TimeZoneInfo.Utc);
            var result = await repository.GetFloorplanWithElementsByGuidAsync(testFloorplan.Guid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testFloorplan.Name, result.Name);
            Assert.Equal(2, result.Elements.Count);
        }
    }

    [Fact]
    public async Task GetFloorplansByRestaurantGuidAsync_ReturnsFloorplansForRestaurant()
    {
        // Arrange
        var restaurantId = 1;
        var restaurantGuid = Guid.NewGuid();
        
        using (var context = new TarabezahDbContext(_options))
        {
            var restaurant = new Restaurant
            {
                Id = restaurantId,
                Guid = restaurantGuid,
                Name = "Test Restaurant",
                CreatedDate = DateTime.UtcNow
            };
            context.Restaurants.Add(restaurant);
            await context.SaveChangesAsync();
            
            context.Floorplans.AddRange(
                new Floorplan
                {
                    Name = "Floorplan 1",
                    RestaurantId = restaurantId,
                    CreatedDate = DateTime.UtcNow
                },
                new Floorplan
                {
                    Name = "Floorplan 2",
                    RestaurantId = restaurantId,
                    CreatedDate = DateTime.UtcNow
                },
                new Floorplan
                {
                    Name = "Other Restaurant Floorplan",
                    RestaurantId = 2,
                    CreatedDate = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new TarabezahDbContext(_options))
        {
            var repository = new FloorplanRepository(context, TimeZoneInfo.Utc);
            var results = await repository.GetFloorplansByRestaurantGuidAsync(restaurantGuid);

            // Assert
            var resultList = results.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, f => Assert.Equal(restaurantId, f.RestaurantId));
        }
    }
} 