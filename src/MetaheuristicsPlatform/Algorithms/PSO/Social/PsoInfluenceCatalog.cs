namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>Representative PSO influence-policy factory catalog.</summary>
public static class PsoInfluenceCatalog
{
    public static IReadOnlyList<IPsoInfluencePolicy> CreateRepresentativeDefaults() =>
        new IPsoInfluencePolicy[]
        {
            new CanonicalBestInfluencePolicy(
                cognitiveCoefficient: 2.05,
                socialCoefficient: 2.05),

            new FullyInformedInfluencePolicy(
                totalAccelerationCoefficient: 4.10),

            new WeightedFullyInformedInfluencePolicy(
                totalAccelerationCoefficient: 4.10,
                EqualInformerWeightProvider.Instance)
        };
}