namespace Contra_Filé.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    
    public bool Active { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}