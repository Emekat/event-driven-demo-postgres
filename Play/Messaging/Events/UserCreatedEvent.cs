using Play.Messaging.Models;

namespace Play.Messaging.Events;
public class UserCreatedEvent : EventBase
{
    public string UserId { get; }
    public string RoleId { get; }
    public string TenantId { get; }

    public UserCreatedEvent(string aggregateId, int version, string userId,
         string roleId, string tenantId)
    {
        AggregateId = aggregateId;
        Version = version;
        UserId = userId;
        TenantId = tenantId;
        RoleId = roleId;
    }
}
