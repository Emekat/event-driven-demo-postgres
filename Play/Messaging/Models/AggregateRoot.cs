namespace Play.Messaging.Models;

public class AggregateRoot
{
    private readonly List<IEvent> _changes = new();
    
    public string Id { get; protected set; }
    public int Version
    {
        get;
        protected set; 
    }

    public IEnumerable<IEvent> GetUncommittedChanges() => _changes;
    public void MarkChangesAsCommitted() => _changes.Clear();

    protected void ApplyChange(IEvent @event)
    {
        ApplyChange(@event, true);
    }

    private void ApplyChange(IEvent @event, bool isNew)
    {
        dynamic d = @event;
        ((dynamic)this).Apply(d);
        
        if (isNew)
            _changes.Add(@event);
    }
}