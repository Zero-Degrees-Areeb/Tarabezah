using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Commands.CreateFloorplan;
using Tarabezah.Application.Dtos;
using Tarabezah.Application.Queries.GetFloorplanById;
using Tarabezah.Web.Controllers;
using Xunit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Application.Common;
using Microsoft.AspNetCore.SignalR;
using Tarabezah.Infrastructure.SignalR;
using Tarabezah.Application.Services;

namespace Tarabezah.Tests.Controllers;

public class FloorplansControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<FloorplansController>> _mockLogger;
    private readonly Mock<IHubContext<TarabezahHub>> _mockHubContext;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly FloorplansController _controller;

    public FloorplansControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<FloorplansController>>();
        _mockHubContext = new Mock<IHubContext<TarabezahHub>>();
        _mockNotificationService = new Mock<INotificationService>();
        _controller = new FloorplansController(_mockMediator.Object, _mockLogger.Object, _mockNotificationService.Object);
    }

    [Fact]
    public async Task GetByGuid_WithExistingGuid_ReturnsOk()
    {
        // Arrange
        var floorplanGuid = Guid.NewGuid();
        var floorplanDto = new FloorplanDto
        {
            Guid = floorplanGuid,
            Name = "Test Floorplan"
        };
        
        _mockMediator
            .Setup(m => m.Send(It.Is<GetFloorplanByIdQuery>(q => q.FloorplanGuid == floorplanGuid), It.IsAny<CancellationToken>()))
            .ReturnsAsync(floorplanDto);

        // Act
        var result = await _controller.GetByGuid(floorplanGuid);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<FloorplanDto>>(okResult.Value);
        Assert.Equal(floorplanGuid, response.Data.Result.Guid);
    }

    [Fact]
    public async Task GetByGuid_WithNonExistingGuid_ReturnsNotFound()
    {
        // Arrange
        var floorplanGuid = Guid.NewGuid();
        
        _mockMediator
            .Setup(m => m.Send(It.Is<GetFloorplanByIdQuery>(q => q.FloorplanGuid == floorplanGuid), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FloorplanDto)null);

        // Act
        var result = await _controller.GetByGuid(floorplanGuid);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<FloorplanDto>>(notFoundResult.Value);
        Assert.False(response.IsSuccess);
    }

    [Fact]
    public async Task Create_WithValidCommand_ReturnsCreatedAtActionWithFloorplan()
    {
        // Arrange
        var floorplanGuid = Guid.NewGuid();
        var command = new CreateFloorplanCommand(
            "Test Floorplan",
            Guid.NewGuid());
        
        var floorplanDto = new FloorplanDto
        {
            Guid = floorplanGuid,
            Name = "Test Floorplan",
            Elements = new List<FloorplanElementResponseDto>()
        };
        
        _mockMediator
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(floorplanDto);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(FloorplansController.GetByGuid), createdAtActionResult.ActionName);
        Assert.Equal(floorplanGuid, createdAtActionResult.RouteValues["guid"]);
        var response = Assert.IsType<ApiResponse<FloorplanDto>>(createdAtActionResult.Value);
        Assert.Equal(floorplanGuid, response.Data.Result.Guid);
        Assert.Equal("Test Floorplan", response.Data.Result.Name);
        Assert.Empty(response.Data.Result.Elements);
    }

    [Fact]
    public async Task Create_WithElementsInCommand_ReturnsCreatedAtActionWithFloorplanAndElements()
    {
        // Arrange
        var elementDto = new FloorplanElementDto
        {
            TableId = "T1",
            ElementGuid = Guid.NewGuid(),
            MinCapacity = 2,
            MaxCapacity = 4,
            X = 100,
            Y = 200,
            Rotation = 0
        };
        
        var command = new CreateFloorplanCommand(
            "Test Floorplan",
            Guid.NewGuid(),
            new List<FloorplanElementDto> { elementDto });
        
        var floorplanGuid = Guid.NewGuid();
        var elementInstanceGuid = Guid.NewGuid();
        
        var floorplanDto = new FloorplanDto
        {
            Guid = floorplanGuid,
            Name = "Test Floorplan",
            RestaurantGuid = Guid.NewGuid(),
            RestaurantName = "Test Restaurant",
            Elements = new List<FloorplanElementResponseDto> 
            { 
                new FloorplanElementResponseDto
                {
                    Guid = elementInstanceGuid,
                    TableId = "T1",
                    ElementGuid = elementDto.ElementGuid,
                    ElementName = "Table",
                    ElementImageUrl = "table.png",
                    ElementType = "Table",
                    MinCapacity = 2,
                    MaxCapacity = 4,
                    X = 100,
                    Y = 200,
                    Rotation = 0,
                    CreatedDate = DateTime.UtcNow
                }
            }
        };
        
        _mockMediator
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(floorplanDto);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(FloorplansController.GetByGuid), createdAtActionResult.ActionName);
        Assert.Equal(floorplanGuid, createdAtActionResult.RouteValues["guid"]);
        var response = Assert.IsType<ApiResponse<FloorplanDto>>(createdAtActionResult.Value);
        Assert.Equal(floorplanGuid, response.Data.Result.Guid);
        Assert.Equal("Test Floorplan", response.Data.Result.Name);
        Assert.Single(response.Data.Result.Elements);
        Assert.Equal("T1", response.Data.Result.Elements[0].TableId);
        Assert.Equal(elementInstanceGuid, response.Data.Result.Elements[0].Guid);
    }

    [Fact]
    public async Task Create_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateFloorplanCommand(
            "Test Floorplan",
            Guid.NewGuid());
        
        _mockMediator
            .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid command"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<FloorplanDto>>(badRequestResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Equal("Invalid command", response.ErrorMessage);
    }
} 