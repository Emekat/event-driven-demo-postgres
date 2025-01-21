using Play.Messaging.Models;
namespace Play.Messaging.Events;
public class UserRegisteredEvent : EventBase
{
    public string UserId { get; }
    public string TenantId { get; }
    public string Name { get; }

    public UserRegisteredEvent(string aggregateId, int version, string userId, string tenantId, string name)
    {
        AggregateId = aggregateId;
        Version = version;
        UserId = userId;
        TenantId = tenantId;
        Name = name;
    }
}
