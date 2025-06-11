//using Microsoft.Extensions.Logging;
//using Moq;
//using Tarabezah.Application.Commands.CreateFloorplan;
//using Tarabezah.Domain.Entities;
//using Tarabezah.Domain.Repositories;
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;
//using Tarabezah.Application.Dtos;

//namespace Tarabezah.Tests.CommandHandlers;

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
//    public async Task Handle_WithValidCommand_ReturnsFloorplanGuid()
//    {
//        // Arrange
//        var restaurantGuid = Guid.NewGuid();
//        var restaurant = new Restaurant { Id = 1, Guid = restaurantGuid, Name = "Test Restaurant" };
        
//        _mockRestaurantRepository
//            .Setup(repo => repo.GetByGuidAsync(restaurantGuid))
//            .ReturnsAsync(restaurant);
            
//        var command = new CreateFloorplanCommand(
//            "Test Floorplan",
//            restaurantGuid);

//        var floorplanGuid = Guid.NewGuid();
//        var savedFloorplan = new Floorplan
//        {
//            Id = 1,
//            Guid = floorplanGuid,
//            Name = command.Name,
//            CreatedDate = DateTime.UtcNow
//        };

//        _mockFloorplanRepository
//            .Setup(repo => repo.AddAsync(It.IsAny<Floorplan>()))
//            .ReturnsAsync(savedFloorplan);

//        // Act
//        var result = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.Equal(floorplanGuid, result);
//        _mockFloorplanRepository.Verify(repo => repo.AddAsync(It.IsAny<Floorplan>()), Times.Once);
//    }
//} 