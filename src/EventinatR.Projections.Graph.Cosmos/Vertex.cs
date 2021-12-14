namespace EventinatR.Projections.Graph.Cosmos;

public abstract class Vertex
{
    public string? Id { get; set; }
    public string? Label { get; set; }
    public string PartitionKey { get; set; } = "PartitionKey";
}
