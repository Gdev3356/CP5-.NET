using Contra_Filé.Domain.Common;

namespace Contra_Filé.Domain.Entities;

public class UserConfig : BaseEntity
{
    private UserConfig() { }
    
    public bool EnableNotifications { get; private set; }

    public string Theme { get; private set; } = "Default";
    
    public Guid UserId { get; private set; }
    
    public UserConfig(Guid userId)
    {
        UserId = userId;
    }
}