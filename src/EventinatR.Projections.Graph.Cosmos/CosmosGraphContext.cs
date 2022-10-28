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

    protected CosmosGraphContext()
        : base(null)
        => _options = new CosmosGraphContextOptions();

    protected virtual ICosmosDbConfigurator ConfigureCosmos(ICosmosDbConfigurator configurator)
        => configurator;

    protected override IGremlinQuerySource ConfigureSource(IConfigurableGremlinQuerySource source)
        => source.UseCosmosDb(c =>
        {
            var configurator = c
                .At(new Uri(_options.Uri, UriKind.Absolute), _options.DatabaseName, _options.GraphName)
                .AuthenticateBy(_options.AuthKey);

            return ConfigureCosmos(configurator)
                ?? throw new InvalidOperationException($"The {GetType().FullName} context did not configure Cosmos.");
        });
}
