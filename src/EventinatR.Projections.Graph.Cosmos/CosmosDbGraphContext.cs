using ExRam.Gremlinq.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventinatR.Projections.Graph.Cosmos;

public abstract class CosmosDbGraphContext : GraphContext
{
    private readonly CosmosDbGraphContextOptions _options;
    private readonly ILogger? _logger;

    protected CosmosDbGraphContext(IOptions<CosmosDbGraphContextOptions> options, ILogger? logger = null)
        : base(options, logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    protected override IGremlinQuerySource ConfigureSource(IConfigurableGremlinQuerySource source)
        => source.UseCosmosDb(c => c
            .At(new Uri(_options.Uri, UriKind.Absolute))
            .OnDatabase(_options.DatabaseName)
            .OnGraph(_options.GraphName)
            .AuthenticateBy(_options.AuthKey)
            .ConfigureWebSocket(ws => ws.ConfigureGremlinClient(c =>
            {
                if (_logger is not null && _logger.IsEnabled(LogLevel.Debug))
                {
                    return c.ObserveResultStatusAttributes((request, attributes) =>
                    {
                        var json = JsonSerializer.Serialize(attributes);
                        _logger?.LogDebug("Executed Gremlin query '{RequestId}': {Attributes}", request.RequestId, json);
                    });
                }

                return c;
            })));
}
