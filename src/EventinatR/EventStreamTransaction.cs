namespace EventinatR;

public record struct EventStreamTransaction(EventStreamVersion Version, long Count)
{
    public static readonly EventStreamTransaction None = new(EventStreamVersion.None, 0);
}
