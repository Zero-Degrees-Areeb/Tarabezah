using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tarabezah.Application.Commands.CreateRestaurant;
using Tarabezah.Application.Common;
using Tarabezah.Application.Commands.CreateAndPublishFloorplans;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Repositories;
using Tarabezah.Web.Controllers;
using Xunit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tarabezah.Application.Services;
using Tarabezah.Application.Dtos.Notifications;
using Microsoft.AspNetCore.SignalR;
using Tarabezah.Infrastructure.SignalR;

namespace Tarabezah.Tests.Controllers;

public class RestaurantsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<RestaurantsController>> _mockLogger;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IHubContext<TarabezahHub>> _mockHubContext;
    private readonly RestaurantsController _controller;

    public RestaurantsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<RestaurantsController>>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockHubContext = new Mock<IHubContext<TarabezahHub>>();
        _controller = new RestaurantsController(
            _mockMediator.Object,
            _mockLogger.Object,
            _mockNotificationService.Object);
    }

    [Fact]
    public async Task CreateFloorplans_WithSuccess_ReturnsCreatedResult()
    {
        // Arrange
        var restaurantGuid = Guid.NewGuid();
        var floorplan1Guid = Guid.NewGuid();
        var floorplan2Guid = Guid.NewGuid();
        
        var floorplans = new List<CreateFloorplanDto>
        {
            new CreateFloorplanDto { Name = "Floor 1" },
            new CreateFloorplanDto { Name = "Floor 2" }
        };
        
        var result = new CreateFloorplansResult
        {
            RestaurantGuid = restaurantGuid,
            TotalFloorplanCount = 2,
            SuccessCount = 2,
            CreatedFloorplans = new List<CreatedFloorplanDto>
            {
                new CreatedFloorplanDto 
                { 
                    Guid = floorplan1Guid, 
                    Name = "Floor 1", 
                    CreatedDate = DateTime.UtcNow
                },
                new CreatedFloorplanDto 
                { 
                    Guid = floorplan2Guid, 
                    Name = "Floor 2", 
                    CreatedDate = DateTime.UtcNow
                }
            }
        };
        
        _mockMediator
            .Setup(m => m.Send(It.Is<CreateFloorplansCommand>(c => 
                c.RestaurantGuid == restaurantGuid && c.Floorplans.Count == 2), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.CreateFloorplans(restaurantGuid, floorplans);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(nameof(RestaurantsController.GetFloorplans), createdAtActionResult.ActionName);
        Assert.Equal(restaurantGuid, createdAtActionResult.RouteValues["guid"]);
        
        var returnValue = Assert.IsType<CreateFloorplansResult>(createdAtActionResult.Value);
        Assert.Equal(2, returnValue.TotalFloorplanCount);
        Assert.Equal(2, returnValue.SuccessCount);
        Assert.Equal(2, returnValue.CreatedFloorplans.Count);
        Assert.Equal(floorplan1Guid, returnValue.CreatedFloorplans[0].Guid);
        Assert.Equal(floorplan2Guid, returnValue.CreatedFloorplans[1].Guid);
    }

    [Fact]
    public async Task CreateFloorplans_WithRestaurantNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var restaurantGuid = Guid.NewGuid();
        
        var floorplans = new List<CreateFloorplanDto>
        {
            new CreateFloorplanDto { Name = "Floor 1" }
        };
        
        var result = new CreateFloorplansResult
        {
            RestaurantGuid = restaurantGuid,
            TotalFloorplanCount = 1,
            SuccessCount = 0
        };
        result.ErrorMessages.Add($"Restaurant with GUID {restaurantGuid} not found");
        
        _mockMediator
            .Setup(m => m.Send(It.Is<CreateFloorplansCommand>(c => 
                c.RestaurantGuid == restaurantGuid), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.CreateFloorplans(restaurantGuid, floorplans);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
        var returnValue = Assert.IsType<CreateFloorplansResult>(notFoundResult.Value);
        Assert.True(returnValue.HasErrors);
        Assert.Contains(returnValue.ErrorMessages, e => e.Contains("not found"));
    }

    [Fact]
    public async Task CreateFloorplans_WithPartialSuccess_ReturnsBadRequestResult()
    {
        // Arrange
        var restaurantGuid = Guid.NewGuid();
        var floorplan1Guid = Guid.NewGuid();
        
        var floorplans = new List<CreateFloorplanDto>
        {
            new CreateFloorplanDto { Name = "Floor 1" },
            new CreateFloorplanDto { Name = "Floor 2" }
        };
        
        var result = new CreateFloorplansResult
        {
            RestaurantGuid = restaurantGuid,
            TotalFloorplanCount = 2,
            SuccessCount = 1,
            CreatedFloorplans = new List<CreatedFloorplanDto>
            {
                new CreatedFloorplanDto 
                { 
                    Guid = floorplan1Guid, 
                    Name = "Floor 1", 
                    CreatedDate = DateTime.UtcNow
                }
            }
        };
        result.ErrorMessages.Add("Error creating floorplan 'Floor 2': Invalid data");
        
        _mockMediator
            .Setup(m => m.Send(It.Is<CreateFloorplansCommand>(c => 
                c.RestaurantGuid == restaurantGuid), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.CreateFloorplans(restaurantGuid, floorplans);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Contains("Failed to create floorplans", badRequestResult.Value.ToString());
    }

    [Fact]
    public async Task CreateFloorplans_WithException_ReturnsBadRequestResult()
    {
        // Arrange
        var restaurantGuid = Guid.NewGuid();
        
        var floorplans = new List<CreateFloorplanDto>
        {
            new CreateFloorplanDto { Name = "Floor 1" }
        };
        
        _mockMediator
            .Setup(m => m.Send(It.Is<CreateFloorplansCommand>(c => 
                c.RestaurantGuid == restaurantGuid), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var actionResult = await _controller.CreateFloorplans(restaurantGuid, floorplans);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Contains("Failed to create floorplans", badRequestResult.Value.ToString());
    }
} 