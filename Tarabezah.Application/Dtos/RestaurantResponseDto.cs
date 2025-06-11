namespace Tarabezah.Application.Dtos;

public class RestaurantResponseDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class RestaurantDetailResponseDto : RestaurantResponseDto
{
    public DateTime ModifiedDate { get; set; }
} 