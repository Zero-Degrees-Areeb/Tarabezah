using MediatR;
using Microsoft.AspNetCore.Http;

namespace Tarabezah.Application.Commands.CreateElementWithImage;

public record CreateElementWithImageCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public IFormFile ImageFile { get; set; } = null!;
    public string TableType { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
} 