namespace EventinatR
{
    public record EventStreamVersion(long Value)
    {
        public static readonly EventStreamVersion None = new(default);

        public static implicit operator EventStreamVersion(int value)
            => new(value);

        public static implicit operator EventStreamVersion(long value)
            => new(value);
    }
}
