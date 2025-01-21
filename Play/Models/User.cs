using Play.Messaging.Events;
using Play.Messaging.Models;

namespace Play.Models;

public class User : AggregateRoot
{
    public string UserId { get; private set; }
    public string TenantId { get; private set; }
    public string Name { get; private set; }

    public User(string userId, string tenantId, string name)
    {
        UserId = userId;
        TenantId = tenantId;
        Name = name;
        ApplyChange(new UserRegisteredEvent(aggregateId: Guid.CreateVersion7().ToString(), version: 0, userId, tenantId, name));
    }
}