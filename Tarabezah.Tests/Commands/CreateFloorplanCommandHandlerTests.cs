//using Moq;
//using Tarabezah.Application.Commands.CreateFloorplan;
//using Tarabezah.Application.Dtos;
//using Tarabezah.Domain.Entities;
//using Tarabezah.Domain.Repositories;
//using Microsoft.Extensions.Logging;
//using Xunit;
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Tarabezah.Tests.Commands;

//public class CreateFloorplanCommandHandlerTests
//{
//    private readonly Mock<IFloorplanRepository> _mockFloorplanRepository;
//    private readonly Mock<IRestaurantRepository> _mockRestaurantRepository;
//    private readonly Mock<IRepository<Element>> _mockElementRepository;
//    private readonly Mock<ILogger<CreateFloorplanCommandHandler>> _mockLogger;
//    private readonly CreateFloorplanCommandHandler _handler;

//    public CreateFloorplanCommandHandlerTests()
//    {
//        _mockFloorplanRepository = new Mock<IFloorplanRepository>();
//        _mockRestaurantRepository = new Mock<IRestaurantRepository>();
//        _mockElementRepository = new Mock<IRepository<Element>>();
//        _mockLogger = new Mock<ILogger<CreateFloorplanCommandHandler>>();
//        _handler = new CreateFloorplanCommandHandler(
//            _mockFloorplanRepository.Object,
//            _mockRestaurantRepository.Object,
//            _mockElementRepository.Object,
//            _mockLogger.Object);
//    }

//    [Fact]
//    public async Task Handle_ValidCommand_CreatesFloorplan()
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

//        var command = new CreateFloorplanCommand(
//            "Test Floorplan",
//            restaurantGuid);

//        // Act
//        var result = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.Equal(floorplanGuid, result);
//        _mockFloorplanRepository.Verify(
//            repo => repo.AddAsync(It.Is<Floorplan>(f => 
//                f.Name == "Test Floorplan" && 
//                f.RestaurantId == 1)), 
//            Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithElements_CreatesFloorplanWithElements()
//    {
//        // Arrange
//        var restaurantGuid = Guid.NewGuid();
//        var restaurant = new Restaurant { Id = 1, Guid = restaurantGuid, Name = "Test Restaurant" };
        
//        _mockRestaurantRepository
//            .Setup(repo => repo.GetByGuidAsync(restaurantGuid))
//            .ReturnsAsync(restaurant);

//        var elementGuid = Guid.NewGuid();
//        var element = new Element { Id = 1, Guid = elementGuid, Name = "Table" };
        
//        _mockElementRepository
//            .Setup(repo => repo.GetByGuidAsync(elementGuid))
//            .ReturnsAsync(element);

//        var floorplanGuid = Guid.NewGuid();
//        _mockFloorplanRepository
//            .Setup(repo => repo.AddAsync(It.IsAny<Floorplan>()))
//            .ReturnsAsync((Floorplan f) => 
//            {
//                f.Id = 1;
//                f.Guid = floorplanGuid;
//                return f;
//            });

//        var elements = new List<FloorplanElementDto>
//        {
//            new FloorplanElementDto
//            {
//                TableId = "T1",
//                ElementGuid = elementGuid,
//                MinCapacity = 2,
//                MaxCapacity = 4,
//                X = 100,
//                Y = 200,
//                Rotation = 0
//            }
//        };

//        var command = new CreateFloorplanCommand(
//            "Test Floorplan",
//            restaurantGuid,
//            elements);

//        // Act
//        var result = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.Equal(floorplanGuid, result);
//        _mockFloorplanRepository.Verify(
//            repo => repo.AddAsync(It.Is<Floorplan>(f => 
//                f.Name == "Test Floorplan" && 
//                f.RestaurantId == 1 &&
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
//    public async Task Handle_WithInvalidRestaurant_ThrowsArgumentException()
//    {
//        // Arrange
//        var restaurantGuid = Guid.NewGuid();
        
//        _mockRestaurantRepository
//            .Setup(repo => repo.GetByGuidAsync(restaurantGuid))
//            .ReturnsAsync((Restaurant)null);

//        var command = new CreateFloorplanCommand(
//            "Test Floorplan",
//            restaurantGuid);

//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentException>(() => 
//            _handler.Handle(command, CancellationToken.None));
//    }

//    [Fact]
//    public async Task Handle_WithInvalidElement_ThrowsArgumentException()
//    {
//        // Arrange
//        var restaurantGuid = Guid.NewGuid();
//        var restaurant = new Restaurant { Id = 1, Guid = restaurantGuid, Name = "Test Restaurant" };
        
//        _mockRestaurantRepository
//            .Setup(repo => repo.GetByGuidAsync(restaurantGuid))
//            .ReturnsAsync(restaurant);

//        var elementGuid = Guid.NewGuid();
        
//        _mockElementRepository
//            .Setup(repo => repo.GetByGuidAsync(elementGuid))
//            .ReturnsAsync((Element)null);

//        var elements = new List<FloorplanElementDto>
//        {
//            new FloorplanElementDto
//            {
//                TableId = "T1",
//                ElementGuid = elementGuid,
//                MinCapacity = 2,
//                MaxCapacity = 4,
//                X = 100,
//                Y = 200,
//                Rotation = 0
//            }
//        };

//        var command = new CreateFloorplanCommand(
//            "Test Floorplan",
//            restaurantGuid,
//            elements);

//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentException>(() => 
//            _handler.Handle(command, CancellationToken.None));
//    }
//} 