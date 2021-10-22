namespace EventinatR.Cosmos;

internal class CosmosEventStoreSerializer : CosmosSerializer
{
    private readonly CosmosEventStoreOptions _options;

    public CosmosEventStoreSerializer(CosmosEventStoreOptions options)
        => _options = options ?? throw new ArgumentNullException(nameof(options));

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            return BinaryData.FromStream(stream).ToObjectFromJson<T>(_options.SerializerOptions);
        }
    }

    public override Stream ToStream<T>(T input)
        => BinaryData.FromObjectAsJson(input, _options.SerializerOptions).ToStream();
}
