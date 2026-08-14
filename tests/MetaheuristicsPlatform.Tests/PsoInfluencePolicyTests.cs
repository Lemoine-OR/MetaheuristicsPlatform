using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoInfluencePolicyTests
{
    [Fact]
    public void CanonicalInfluence_IsReproducibleForFixedSeed()
    {
        PsoSocialContext context = CreateFullyConnectedContext();
        var policy =
            new CanonicalBestInfluencePolicy(
                2.05,
                2.05);

        double[] first = new double[2];
        double[] second = new double[2];

        policy.ComputeAttraction(
            0,
            context,
            new Xoshiro256StarStarRandomSource(123UL),
            first);

        policy.ComputeAttraction(
            0,
            context,
            new Xoshiro256StarStarRandomSource(123UL),
            second);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Fips_UsesAllTopologyDefinedInformers()
    {
        double[][] positions =
        {
            new[] { 0.0 },
            new[] { 0.0 },
            new[] { 0.0 }
        };

        double[][] personalBest =
        {
            new[] { 1.0 },
            new[] { 2.0 },
            new[] { 3.0 }
        };

        double[] fitness = { 3.0, 2.0, 1.0 };

        var topology =
            new FullyConnectedTopology(
                includeSelf: true);

        NeighborhoodGraph graph =
            topology.CreateGraph(
                new PsoTopologyContext(
                    3,
                    0,
                    OptimizationSense.Minimize),
                new Xoshiro256StarStarRandomSource(1UL));

        var context =
            new PsoSocialContext(
                positions,
                personalBest,
                fitness,
                graph,
                OptimizationSense.Minimize);

        var fips =
            new FullyInformedInfluencePolicy(
                totalAccelerationCoefficient: 4.1);

        double[] attraction = new double[1];

        fips.ComputeAttraction(
            0,
            context,
            new ConstantHalfRandomSource(),
            attraction);

        // 4.1 / 3 * 0.5 * (1 + 2 + 3) = 4.1
        Assert.Equal(4.1, attraction[0], 12);
    }

    [Fact]
    public void Fips_RespectsTopologyRatherThanGlobalPopulation()
    {
        double[][] positions =
        {
            new[] { 0.0 },
            new[] { 0.0 },
            new[] { 0.0 },
            new[] { 0.0 },
            new[] { 0.0 }
        };

        double[][] personalBest =
        {
            new[] { 1.0 },
            new[] { 2.0 },
            new[] { 1000.0 },
            new[] { 1000.0 },
            new[] { 5.0 }
        };

        double[] fitness = { 1, 2, 3, 4, 5 };

        var ring =
            new RingTopology(
                radius: 1,
                includeSelf: true);

        NeighborhoodGraph graph =
            ring.CreateGraph(
                new PsoTopologyContext(
                    5,
                    0,
                    OptimizationSense.Minimize),
                new Xoshiro256StarStarRandomSource(1UL));

        var context =
            new PsoSocialContext(
                positions,
                personalBest,
                fitness,
                graph,
                OptimizationSense.Minimize);

        var fips =
            new FullyInformedInfluencePolicy(3.0);

        double[] attraction = new double[1];

        fips.ComputeAttraction(
            0,
            context,
            new ConstantHalfRandomSource(),
            attraction);

        // Particle 0 informers are 0,1,4 only.
        // coefficient = 3/3 = 1; random=.5; deltas 1,2,5 => 4.
        Assert.Equal(4.0, attraction[0], 12);
    }

    [Fact]
    public void WeightedFullyInformed_EqualWeightsMatchesFips()
    {
        PsoSocialContext context =
            CreateFullyConnectedContext();

        var fips =
            new FullyInformedInfluencePolicy(4.1);

        var weighted =
            new WeightedFullyInformedInfluencePolicy(
                4.1,
                EqualInformerWeightProvider.Instance);

        double[] first = new double[2];
        double[] second = new double[2];

        fips.ComputeAttraction(
            1,
            context,
            new Xoshiro256StarStarRandomSource(99UL),
            first);

        weighted.ComputeAttraction(
            1,
            context,
            new Xoshiro256StarStarRandomSource(99UL),
            second);

        Assert.Equal(first, second);
    }

    private static PsoSocialContext CreateFullyConnectedContext()
    {
        double[][] positions =
        {
            new[] { 0.0, 0.0 },
            new[] { 1.0, 1.0 },
            new[] { 2.0, 2.0 }
        };

        double[][] personalBest =
        {
            new[] { 0.5, -0.5 },
            new[] { 0.0, 0.0 },
            new[] { -1.0, 1.0 }
        };

        double[] fitness =
        {
            2.0,
            1.0,
            3.0
        };

        var topology =
            new FullyConnectedTopology(
                includeSelf: true);

        NeighborhoodGraph graph =
            topology.CreateGraph(
                new PsoTopologyContext(
                    3,
                    0,
                    OptimizationSense.Minimize),
                new Xoshiro256StarStarRandomSource(1UL));

        return new PsoSocialContext(
            positions,
            personalBest,
            fitness,
            graph,
            OptimizationSense.Minimize);
    }

    private sealed class ConstantHalfRandomSource : IRandomSource
    {
        public ulong Seed => 0UL;

        public ulong NextUInt64() =>
            0x8000000000000000UL;

        public double NextDouble() => 0.5;

        public int NextInt32(int exclusiveMax) =>
            exclusiveMax <= 0
                ? throw new ArgumentOutOfRangeException(nameof(exclusiveMax))
                : exclusiveMax / 2;

        public int NextInt32(
            int inclusiveMin,
            int exclusiveMax)
        {
            if (inclusiveMin >= exclusiveMax)
            {
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            }

            return inclusiveMin +
                ((exclusiveMax - inclusiveMin) / 2);
        }

        public void Fill(Span<byte> buffer) =>
            buffer.Fill(128);
    }
}