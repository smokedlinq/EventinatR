namespace EventinatR
{
    public record EventStreamVersion(long Value)
    {
        public static readonly EventStreamVersion None = new(default(long));
    }
}
