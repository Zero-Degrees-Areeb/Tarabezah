using System;
using System.Collections.Generic;
using Tarabezah.Application.Dtos;

namespace Tarabezah.Application.Commands.CreateAndPublishFloorplans;

public class CreateFloorplansResult
{
    public Guid RestaurantGuid { get; set; }
    public int TotalFloorplanCount { get; set; }
    public int SuccessCount { get; set; }
    public int DeletedCount { get; set; }
    public List<CreatedFloorplanDto> CreatedFloorplans { get; set; } = new List<CreatedFloorplanDto>();
    public bool HasErrors => ErrorMessages.Count > 0;
    public List<string> ErrorMessages { get; set; } = new List<string>();
}

public class CreatedFloorplanDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public List<FloorplanElementResponseDto> Elements { get; set; } = new List<FloorplanElementResponseDto>();
} 