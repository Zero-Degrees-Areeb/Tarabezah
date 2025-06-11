namespace Tarabezah.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }  // Auto-incremented PK
    public Guid Guid { get; set; } = Guid.NewGuid();  // Public identifier
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
} 