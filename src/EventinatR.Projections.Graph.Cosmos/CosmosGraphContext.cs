using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Providers.CosmosDb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventinatR.Projections.Graph.Cosmos;

public abstract class CosmosGraphContext : GraphContext
{
    private readonly CosmosGraphContextOptions _options;
    private readonly ILogger? _logger;

    protected CosmosGraphContext(IOptions<CosmosGraphContextOptions> options, ILogger? logger = null)
        : base(logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    protected virtual ICosmosDbConfigurator ConfigureCosmos(ICosmosDbConfigurator configurator)
        => configurator;

    protected override IGremlinQuerySource ConfigureSource(IConfigurableGremlinQuerySource source)
        => source.UseCosmosDb(c =>
        {
            var configurator = c
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
                }));

            return ConfigureCosmos(configurator)
                ?? throw new InvalidOperationException($"The {GetType().FullName} context did not configure Cosmos.");
        });
}
