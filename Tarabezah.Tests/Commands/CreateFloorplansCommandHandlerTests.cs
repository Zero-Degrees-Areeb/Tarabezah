//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Tarabezah.Application.Commands.CreateAndPublishFloorplans;
//using Tarabezah.Application.Dtos;
//using Tarabezah.Domain.Entities;
//using Tarabezah.Domain.Repositories;
//using Xunit;

//namespace Tarabezah.Tests.Commands;

//public class CreateFloorplansCommandHandlerTests
//{
//    private readonly Mock<IFloorplanRepository> _mockFloorplanRepository;
//    private readonly Mock<IRestaurantRepository> _mockRestaurantRepository;
//    private readonly Mock<IRepository<Element>> _mockElementRepository;
//    private readonly Mock<ILogger<CreateFloorplansCommandHandler>> _mockLogger;
//    private readonly CreateFloorplansCommandHandler _handler;

//    public CreateFloorplansCommandHandlerTests()
//    {
//        _mockFloorplanRepository = new Mock<IFloorplanRepository>();
//        _mockRestaurantRepository = new Mock<IRestaurantRepository>();
//        _mockElementRepository = new Mock<IRepository<Element>>();
//        _mockLogger = new Mock<ILogger<CreateFloorplansCommandHandler>>();
//        _handler = new CreateFloorplansCommandHandler(
//            _mockFloorplanRepository.Object,
//            _mockRestaurantRepository.Object,
//            _mockElementRepository.Object,
//            _mockLogger.Object);
//    }

//    [Fact]
//    public async Task Handle_RestaurantNotFound_ReturnsErrorResult()
//    {
//        // Arrange
//        var restaurantGuid = Guid.NewGuid();
        
//        _mockRestaurantRepository
//            .Setup(repo => repo.GetByGuidAsync(restaurantGuid))
//            .ReturnsAsync((Restaurant)null);

//        var floorplans = new List<CreateFloorplanDto>
//        {
//            new CreateFloorplanDto { Name = "Floor 1" }
//        };

//        var command = new CreateFloorplansCommand(restaurantGuid, floorplans);

//        // Act
//        var result = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.True(result.HasErrors);
//        Assert.Contains(result.ErrorMessages, e => e.Contains("not found"));
//        Assert.Equal(1, result.TotalFloorplanCount);
//        Assert.Equal(0, result.SuccessCount);
//        Assert.Empty(result.CreatedFloorplans);
//    }

//    [Fact]
//    public async Task Handle_SingleFloorplanWithoutElements_CreatesFloorplan()
//    {
//        // Arrange
//        var restaurantGuid = Guid.NewGuid();
//        var restaurant = new Restaurant { Id = 1, Guid = restaurantGuid, Name = "Test Restaurant" };
        
//        _mockRestaurantRepository
//            .Setup(repo => repo.GetByGuidAsync(restaurantGuid))
//            .ReturnsAsync(restaurant);

//        var floorplanGuid = Guid.NewGuid();
//        _mockFloorplanRepository
//            .Setup(repo => repo.AddAsync(It.IsAny<Floorplan>()))
//            .ReturnsAsync((Floorplan f) => 
//            {
//                f.Id = 1;
//                f.Guid = floorplanGuid;
//                return f;
//            });

//        var floorplans = new List<CreateFloorplanDto>
//        {
//            new CreateFloorplanDto { Name = "Floor 1" }
//        };

//        var command = new CreateFloorplansCommand(restaurantGuid, floorplans);

//        // Act
//        var result = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.False(result.HasErrors);
//        Assert.Equal(1, result.TotalFloorplanCount);
//        Assert.Equal(1, result.SuccessCount);
//        Assert.Single(result.CreatedFloorplans);
//        Assert.Equal(floorplanGuid, result.CreatedFloorplans[0].Guid);
//        Assert.Equal("Floor 1", result.CreatedFloorplans[0].Name);
        
//        _mockFloorplanRepository.Verify(
//            repo => repo.AddAsync(It.Is<Floorplan>(f => 
//                f.Name == "Floor 1" &&
//                f.RestaurantId == 1)), 
//            Times.Once);
//    }

//    [Fact]
//    public async Task Handle_FloorplanWithElements_CreatesFloorplanWithElements()
//    {
//        // Arrange
//        var restaurantGuid = Guid.NewGuid();
//        var restaurant = new Restaurant { Id = 1, Guid = restaurantGuid, Name = "Test Restaurant" };
        
//        _mockRestaurantRepository
//            .Setup(repo => repo.GetByGuidAsync(restaurantGuid))
//            .ReturnsAsync(restaurant);

//        var elementGuid = Guid.NewGuid();
//        var element = new Element { 
//            Id = 1, 
//            Guid = elementGuid, 
//            Name = "Table", 
//            ImageUrl = "table.png",
//            TableType = Domain.Enums.TableType.Round 
//        };
        
//        _mockElementRepository
//            .Setup(repo => repo.GetByGuidAsync(elementGuid))
//            .ReturnsAsync(element);

//        var floorplanGuid = Guid.NewGuid();
//        var elementInstanceGuid = Guid.NewGuid();
        
//        _mockFloorplanRepository
//            .Setup(repo => repo.AddAsync(It.IsAny<Floorplan>()))
//            .Returns<Floorplan>(async f => 
//            {
//                f.Id = 1;
//                f.Guid = floorplanGuid;
                
//                // Set GUIDs for all elements and establish relationships
//                foreach (var e in f.Elements)
//                {
//                    e.Id = 1;
//                    e.Guid = elementInstanceGuid;
//                    e.Element = element;
//                }
                
//                return f;
//            });

//        var floorplans = new List<CreateFloorplanDto>
//        {
//            new CreateFloorplanDto { 
//                Name = "Floor 1",
//                Elements = new List<FloorplanElementDto>
//                {
//                    new FloorplanElementDto
//                    {
//                        TableId = "T1",
//                        ElementGuid = elementGuid,
//                        MinCapacity = 2,
//                        MaxCapacity = 4,
//                        X = 100,
//                        Y = 200,
//                        Rotation = 0
//                    }
//                }
//            }
//        };

//        var command = new CreateFloorplansCommand(restaurantGuid, floorplans);

//        // Act
//        var result = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.False(result.HasErrors);
//        Assert.Equal(1, result.TotalFloorplanCount);
//        Assert.Equal(1, result.SuccessCount);
//        Assert.Single(result.CreatedFloorplans);
//        Assert.Equal(floorplanGuid, result.CreatedFloorplans[0].Guid);
//        Assert.Single(result.CreatedFloorplans[0].Elements);
        
//        var resultElement = result.CreatedFloorplans[0].Elements[0];
//        Assert.Equal(elementInstanceGuid, resultElement.Guid);
//        Assert.Equal("T1", resultElement.TableId);
//        Assert.Equal(elementGuid, resultElement.ElementGuid);
//        Assert.Equal("Table", resultElement.ElementName);
//        Assert.Equal("table.png", resultElement.ElementImageUrl);
//        Assert.Equal("Round", resultElement.ElementType);
//        Assert.Equal(2, resultElement.MinCapacity);
//        Assert.Equal(4, resultElement.MaxCapacity);
//        Assert.Equal(100, resultElement.X);
//        Assert.Equal(200, resultElement.Y);
//        Assert.Equal(0, resultElement.Rotation);
        
//        _mockFloorplanRepository.Verify(
//            repo => repo.AddAsync(It.Is<Floorplan>(f => 
//                f.Name == "Floor 1" && 
//                f.Elements.Count == 1 &&
//                f.Elements.First().TableId == "T1" &&
//                f.Elements.First().ElementId == 1 &&
//                f.Elements.First().MinCapacity == 2 &&
//                f.Elements.First().MaxCapacity == 4 &&
//                f.Elements.First().X == 100 &&
//                f.Elements.First().Y == 200 &&
//                f.Elements.First().Rotation == 0)), 
//            Times.Once);
//    }

//    [Fact]
//    public async Task Handle_MultipleFloorplans_CreatesAllFloorplans()
//    {
//        // Arrange
//        var restaurantGuid = Guid.NewGuid();
//        var restaurant = new Restaurant { Id = 1, Guid = restaurantGuid, Name = "Test Restaurant" };
        
//        _mockRestaurantRepository
//            .Setup(repo => repo.GetByGuidAsync(restaurantGuid))
//            .ReturnsAsync(restaurant);

//        var floorplan1Guid = Guid.NewGuid();
//        var floorplan2Guid = Guid.NewGuid();
//        var callCount = 0;
        
//        _mockFloorplanRepository
//            .Setup(repo => repo.AddAsync(It.IsAny<Floorplan>()))
//            .Returns<Floorplan>(async f => 
//            {
//                f.Id = callCount + 1;
//                f.Guid = callCount == 0 ? floorplan1Guid : floorplan2Guid;
//                callCount++;
//                return f;
//            });

//        var floorplans = new List<CreateFloorplanDto>
//        {
//            new CreateFloorplanDto { Name = "Floor 1" },
//            new CreateFloorplanDto { Name = "Floor 2" }
//        };

//        var command = new CreateFloorplansCommand(restaurantGuid, floorplans);

//        // Act
//        var result = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.False(result.HasErrors);
//        Assert.Equal(2, result.TotalFloorplanCount);
//        Assert.Equal(2, result.SuccessCount);
//        Assert.Equal(2, result.CreatedFloorplans.Count);
//        Assert.Equal(floorplan1Guid, result.CreatedFloorplans[0].Guid);
//        Assert.Equal(floorplan2Guid, result.CreatedFloorplans[1].Guid);
//        Assert.Equal("Floor 1", result.CreatedFloorplans[0].Name);
//        Assert.Equal("Floor 2", result.CreatedFloorplans[1].Name);
        
//        _mockFloorplanRepository.Verify(
//            repo => repo.AddAsync(It.IsAny<Floorplan>()), 
//            Times.Exactly(2));
//    }

//    [Fact]
//    public async Task Handle_WithElementError_ContinuesProcessingAndReturnsPartialSuccess()
//    {
//        // Arrange
//        var restaurantGuid = Guid.NewGuid();
//        var restaurant = new Restaurant { Id = 1, Guid = restaurantGuid, Name = "Test Restaurant" };
        
//        _mockRestaurantRepository
//            .Setup(repo => repo.GetByGuidAsync(restaurantGuid))
//            .ReturnsAsync(restaurant);

//        var existingElementGuid = Guid.NewGuid();
//        var nonExistingElementGuid = Guid.NewGuid();
//        var element = new Element { Id = 1, Guid = existingElementGuid, Name = "Table" };
        
//        _mockElementRepository
//            .Setup(repo => repo.GetByGuidAsync(existingElementGuid))
//            .ReturnsAsync(element);
            
//        _mockElementRepository
//            .Setup(repo => repo.GetByGuidAsync(nonExistingElementGuid))
//            .ReturnsAsync((Element)null);

//        var floorplan1Guid = Guid.NewGuid();
        
//        _mockFloorplanRepository
//            .Setup(repo => repo.AddAsync(It.IsAny<Floorplan>()))
//            .ReturnsAsync((Floorplan f) => 
//            {
//                f.Id = 1;
//                f.Guid = floorplan1Guid;
//                return f;
//            });

//        var floorplans = new List<CreateFloorplanDto>
//        {
//            new CreateFloorplanDto { 
//                Name = "Floor 1",
//                Elements = new List<FloorplanElementDto>
//                {
//                    new FloorplanElementDto
//                    {
//                        TableId = "T1",
//                        ElementGuid = existingElementGuid,
//                        MinCapacity = 2,
//                        MaxCapacity = 4,
//                        X = 100,
//                        Y = 200,
//                        Rotation = 0
//                    }
//                }
//            },
//            new CreateFloorplanDto { 
//                Name = "Floor 2",
//                Elements = new List<FloorplanElementDto>
//                {
//                    new FloorplanElementDto
//                    {
//                        TableId = "T1",
//                        ElementGuid = nonExistingElementGuid,
//                        MinCapacity = 2,
//                        MaxCapacity = 4,
//                        X = 100,
//                        Y = 200,
//                        Rotation = 0
//                    }
//                }
//            }
//        };

//        var command = new CreateFloorplansCommand(restaurantGuid, floorplans);

//        // Act
//        var result = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.True(result.HasErrors);
//        Assert.Equal(2, result.TotalFloorplanCount);
//        Assert.Equal(1, result.SuccessCount);
//        Assert.Single(result.CreatedFloorplans);
//        Assert.Equal(floorplan1Guid, result.CreatedFloorplans[0].Guid);
//        Assert.Equal("Floor 1", result.CreatedFloorplans[0].Name);
//        Assert.Contains(result.ErrorMessages, e => e.Contains("Floor 2") && e.Contains("not found"));
        
//        _mockFloorplanRepository.Verify(
//            repo => repo.AddAsync(It.IsAny<Floorplan>()), 
//            Times.Once);
//    }
//} 