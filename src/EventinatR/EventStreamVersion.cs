namespace EventinatR
{
    public record EventStreamVersion(long Version)
    {
        public static readonly EventStreamVersion None = new(default(long));
    }
}
