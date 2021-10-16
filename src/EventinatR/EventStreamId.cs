namespace EventinatR
{
    public record EventStreamId(string Value)
    {
        public static readonly EventStreamId None = new(string.Empty);
    }
}
