using MetaheuristicsPlatform.Graphs;

namespace MetaheuristicsPlatform.Tests;

public sealed class NeighborhoodGraphTests
{
    [Fact]
    public void Builder_CreatesSortedSymmetricNeighborhoods()
    {
        var builder = new UndirectedGraphBuilder(4);
        builder.AddEdge(0, 3);
        builder.AddEdge(0, 1);
        builder.AddEdge(0, 1);
        builder.AddEdge(2, 2);

        NeighborhoodGraph graph = builder.Build();

        Assert.Equal(new[] { 1, 3 }, graph.GetNeighbors(0).ToArray());
        Assert.Equal(new[] { 0 }, graph.GetNeighbors(1).ToArray());
        Assert.Equal(new[] { 2 }, graph.GetNeighbors(2).ToArray());
        Assert.Equal(new[] { 0 }, graph.GetNeighbors(3).ToArray());
        Assert.Equal(3, graph.EdgeCount);
        Assert.Equal(1, graph.SelfLoopCount);
    }
}