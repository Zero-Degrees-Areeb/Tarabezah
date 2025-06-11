using Tarabezah.Domain.Common;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Domain.Entities;

public class Element : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    public TableType TableType { get; set; }
    public ElementPurpose Purpose { get; set; }

    public ICollection<FloorplanElementInstance> UsedIn { get; set; } = new List<FloorplanElementInstance>();
} 