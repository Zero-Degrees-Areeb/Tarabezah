namespace Tarabezah.Application.Dtos;

public class TableDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public double XPosition { get; set; }
    public double YPosition { get; set; }
} 