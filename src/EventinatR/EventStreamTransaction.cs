namespace EventinatR;

public record EventStreamTransaction(EventStreamVersion Version, long Count)
{
    public static readonly EventStreamTransaction None = new(EventStreamVersion.None, 0);
}
