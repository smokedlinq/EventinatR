namespace EventinatR.Projections.Graph.Cosmos;

public class CosmosDbGraphContextOptions : GraphContextOptions
{
    public string Uri { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string GraphName { get; set; } = string.Empty;
    public string AuthKey { get; set; } = string.Empty;
}
