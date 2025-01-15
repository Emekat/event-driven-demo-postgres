using Play.Messaging.Events;
using Play.Messaging.Models;

namespace Play.Models;

public class User : AggregateRoot
{
    public string UserId { get; private set; }
    public string RoleId { get; private set; }
    public string TenantId { get; private set; }

    public User(string userId, string roleId, string tenantId)
    {
        ApplyChange(new UserCreatedEvent(Guid.NewGuid().ToString(), 0, userId, roleId, tenantId));
    }

    public void Apply(UserCreatedEvent @event)
    {
        Id = @event.AggregateId;
        UserId = @event.UserId;
        RoleId = @event.RoleId;
        TenantId = @event.TenantId;
    }
}