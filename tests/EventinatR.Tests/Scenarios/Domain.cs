namespace EventinatR.Tests.Scenarios;

public record GroupMember(string Name);
public record GroupId(string Name)
{
    public static readonly GroupId None = new(string.Empty);
}

public record GroupEvent
{
    public record Created(GroupId Id) : GroupEvent;
    public record AddedMember(GroupId Id, GroupMember Member) : GroupEvent;
    public record RemovedMember(GroupId Id, GroupMember Member) : GroupEvent;
}

public class Group
{
    private record GroupState(GroupId Id, IEnumerable<GroupMember> Members);

    public GroupId Id { get; private set; }
    public IEnumerable<GroupMember> Members => _members.AsEnumerable();

    private readonly List<GroupMember> _members = new();
    private readonly List<GroupEvent> _uncommittedEvents = new();

    private Group()
        => Id = GroupId.None;

    private void AddEvent(GroupEvent e)
    {
        ApplyEvent(e);
        _uncommittedEvents.Add(e);
    }

    private void ApplyEvent(GroupEvent e)
    {
        switch (e)
        {
            case GroupEvent.Created created:
                Apply(created);
                break;
            case GroupEvent.AddedMember addedMember:
                Apply(addedMember);
                break;
            case GroupEvent.RemovedMember removedMember:
                Apply(removedMember);
                break;
            default:
                throw new InvalidOperationException($"Unsupported event: {e.GetType().FullName}");
        }
    }

    public static Group Create(string name)
    {
        var group = new Group();
        group.AddEvent(new GroupEvent.Created(new GroupId(name)));
        return group;
    }

    private void Apply(GroupEvent.Created e)
        => Id = e.Id;

    public void AddMember(string name)
    {
        if (!_members.Any(x => x.Name == name))
        {
            var member = new GroupMember(name);
            var e = new GroupEvent.AddedMember(Id, member);
            AddEvent(e);
        }
    }

    private void Apply(GroupEvent.AddedMember e)
        => _members.Add(e.Member);

    public void RemoveMember(string name)
    {
        var member = _members.FirstOrDefault(x => x.Name == name);

        if (member is not null)
        {
            var e = new GroupEvent.RemovedMember(Id, member);
            AddEvent(e);
        }
    }

    private void Apply(GroupEvent.RemovedMember e)
        => _members.Remove(e.Member);

    public static async Task<Group?> ReadAsync(EventStream stream)
    {
        var group = new Group();
        var snapshot = await stream.ReadSnapshotAsync<GroupState>();
        var state = snapshot.State;

        if (state is not null)
        {
            group.Id = state.Id;
            group._members.AddRange(state.Members);
        }

        var events = await snapshot.ReadAsync().ToListAsync();

        if (!events.Any() && state is null)
        {
            return null;
        }

        foreach (var e in events)
        {
            if (!e.TryConvert<GroupEvent>(out var groupEvent))
            {
                throw new InvalidOperationException($"The event {e.Data} is not supported.");
            }

            group.ApplyEvent(groupEvent);
        }

        return group;
    }

    public async Task SaveAsync(EventStream stream)
    {
        if (_uncommittedEvents.Count > 0)
        {
            _ = await stream.AppendAsync(_uncommittedEvents, new GroupState(Id, _members));
            _uncommittedEvents.Clear();
        }
    }
}
