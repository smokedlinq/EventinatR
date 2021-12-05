using ExRam.Gremlinq.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventinatR.Projections.Graph.Cosmos;

public abstract class GraphContext : IStartGremlinQuery
{
    private readonly Lazy<IGremlinQuerySource> _source;

    public GraphContext(IOptions<GraphContextOptions> options, ILogger? logger = null)
    {
        var environment = GremlinQuerySource.g
            .ConfigureEnvironment(env =>
            {
                if (logger is not null)
                {
                    env = env.UseLogger(logger);
                }

                return ConfigureEnvironment(env);
            })
            ?? throw new InvalidOperationException($"The {GetType().FullName} context did not configure the environment.");

        _source = new Lazy<IGremlinQuerySource>(() => ConfigureSource(environment)
            ?? throw new InvalidOperationException($"The {GetType().FullName} context did not configure the source."));
    }

    protected IGremlinQuerySource Source => _source.Value;

    protected abstract IGremlinQueryEnvironment ConfigureEnvironment(IGremlinQueryEnvironment env);

    protected abstract IGremlinQuerySource ConfigureSource(IConfigurableGremlinQuerySource source);

    public virtual IVertexGremlinQuery<TVertex> AddV<TVertex>(TVertex vertex)
        => Source.AddV(vertex);

    public virtual IVertexGremlinQuery<TVertex> AddV<TVertex>() where TVertex : new()
        => Source.AddV<TVertex>();

    public virtual IEdgeGremlinQuery<TEdge> AddE<TEdge>(TEdge edge)
        => Source.AddE(edge);

    public virtual IEdgeGremlinQuery<TEdge> AddE<TEdge>() where TEdge : new()
        => Source.AddE<TEdge>();

    public virtual IVertexGremlinQuery<object> V(params object[] ids)
        => Source.V(ids);

    public virtual IVertexGremlinQuery<TVertex> V<TVertex>(params object[] ids)
        => Source.V<TVertex>(ids);

    public virtual IValueGremlinQuery<TElement> Inject<TElement>(params TElement[] elements)
        => Source.Inject(elements);

    public virtual IVertexGremlinQuery<TNewVertex> ReplaceV<TNewVertex>(TNewVertex vertex)
        => Source.ReplaceV(vertex);

    public virtual IEdgeGremlinQuery<TNewEdge> ReplaceE<TNewEdge>(TNewEdge edge)
        => Source.ReplaceE(edge);

    public virtual IEdgeGremlinQuery<TEdge> E<TEdge>(params object[] ids)
        => Source.E<TEdge>(ids);

    public virtual IEdgeGremlinQuery<object> E(params object[] ids)
        => Source.E(ids);
}
