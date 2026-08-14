using MetaheuristicsPlatform.Algorithms.PSO.Social;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoSocialDescriptorTests
{
    [Fact]
    public void FipsDescriptor_ContainsOriginalDoi()
    {
        var fips =
            new FullyInformedInfluencePolicy(4.1);

        Assert.True(
            fips.Descriptor.IsPublishedExactStructure);

        Assert.Contains(
            fips.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1109/TEVC.2004.826074");
    }

    [Fact]
    public void GenericWeightedPolicy_IsNotClaimedAsExactSfipso()
    {
        var policy =
            new WeightedFullyInformedInfluencePolicy(
                4.1,
                EqualInformerWeightProvider.Instance);

        Assert.False(
            policy.Descriptor.IsPublishedExactStructure);

        Assert.DoesNotContain(
            "sfipso",
            policy.Descriptor.Id,
            StringComparison.OrdinalIgnoreCase);
    }
}