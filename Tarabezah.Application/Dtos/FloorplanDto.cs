using System;
using System.Collections.Generic;

namespace Tarabezah.Application.Dtos;

public class FloorplanDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid RestaurantGuid { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public List<FloorplanElementResponseDto> Elements { get; set; } = new List<FloorplanElementResponseDto>();
} 