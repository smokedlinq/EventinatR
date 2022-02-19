using System.Runtime.CompilerServices;

namespace EventinatR.Cosmos;

internal class CosmosEventStreamReader
{
    private readonly Container _container;
    private readonly PartitionKey _partitionKey;

    public CosmosEventStreamReader(Container container, PartitionKey partitionKey, EventStreamVersion? version = null)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _partitionKey = partitionKey;
        Version = version ?? EventStreamVersion.None;
    }

    public EventStreamVersion Version { get; }

    public async IAsyncEnumerable<Event> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.type = @type AND c.version > @version ORDER BY c.version")
            .WithParameter("@type", DocumentTypes.Event)
            .WithParameter("@version", Version.Value);

        var options = new QueryRequestOptions
        {
            PartitionKey = _partitionKey
        };

        var iterator = _container.GetItemQueryIterator<EventDocument>(query, null, options);

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);

            foreach (var doc in page)
            {
                yield return doc.ToEvent();
            }
        }
    }
}
