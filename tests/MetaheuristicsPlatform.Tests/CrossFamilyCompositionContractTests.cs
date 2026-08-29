using MetaheuristicsPlatform.ReferenceGrade;

namespace MetaheuristicsPlatform.Tests;

public sealed class CrossFamilyCompositionContractTests
{
    [Fact]
    public void ValidateAcyclic_RejectsCycles()
    {
        var nodes = new[]
        {
            new ReferenceCompositionNode("search", "search-id", ReferenceCompositionRole.PrimarySearch),
            new ReferenceCompositionNode("repair", "repair-id", ReferenceCompositionRole.ExactRepair)
        };

        var valid = new CrossFamilyCompositionContract(
            nodes,
            new[] { new ReferenceCompositionEdge("search", "repair") });
        valid.ValidateAcyclic();

        Assert.Throws<ArgumentException>(() => new CrossFamilyCompositionContract(
            nodes,
            new[]
            {
                new ReferenceCompositionEdge("search", "repair"),
                new ReferenceCompositionEdge("repair", "search")
            }));
    }
}
