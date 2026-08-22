using MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdvancedGeneticAlgorithmTests
{
    [Fact]
    public void ComponentIdsAreStableAndDistinct()
    {
        string[] ids =
        [
            GeneticAlgorithmComponentIds.TournamentSelection,
            GeneticAlgorithmComponentIds.TruncationSelection,
            GeneticAlgorithmComponentIds.LinearRankingSelection,
            GeneticAlgorithmComponentIds.ExponentialRankingSelection,
            GeneticAlgorithmComponentIds.ExplicitFitnessProportionateSelection,
            GeneticAlgorithmComponentIds.OnePointCrossover,
            GeneticAlgorithmComponentIds.TwoPointCrossover,
            GeneticAlgorithmComponentIds.UniformCrossover,
            GeneticAlgorithmComponentIds.PartiallyMappedCrossover,
            GeneticAlgorithmComponentIds.OrderCrossover,
            GeneticAlgorithmComponentIds.BoundedSimulatedBinaryCrossover,
            GeneticAlgorithmComponentIds.BitFlipMutation,
            GeneticAlgorithmComponentIds.IntegerRandomResetMutation,
            GeneticAlgorithmComponentIds.SwapMutation,
            GeneticAlgorithmComponentIds.InversionMutation,
            GeneticAlgorithmComponentIds.BoundedGaussianMutation,
            GeneticAlgorithmComponentIds.BoundedPolynomialMutation,
            GeneticAlgorithmComponentIds.GenerationalElitistReplacement,
            GeneticAlgorithmComponentIds.SteadyStateReplacement
        ];

        Assert.Equal(ids.Length, ids.Distinct().Count());
        Assert.All(ids, id => Assert.StartsWith("ga.", id));
        Assert.Equal("ga.crossover.pmx", GeneticAlgorithmComponentIds.PartiallyMappedCrossover);
        Assert.Equal("ga.crossover.ox1", GeneticAlgorithmComponentIds.OrderCrossover);
    }

    [Fact]
    public void TruncationSelectionHonorsMinimization()
    {
        var selector =
            new TruncationGeneticParentSelectionMethod<int>(0.5);

        int selected =
            selector.SelectParent(
                Population(9.0, 1.0, 5.0, 2.0),
                OptimizationSense.Minimize,
                new ScriptedRandomSource(ints: [1]));

        Assert.Equal(3, selected);
    }

    [Fact]
    public void TruncationSelectionHonorsMaximization()
    {
        var selector =
            new TruncationGeneticParentSelectionMethod<int>(0.5);

        int selected =
            selector.SelectParent(
                Population(9.0, 1.0, 5.0, 2.0),
                OptimizationSense.Maximize,
                new ScriptedRandomSource(ints: [0]));

        Assert.Equal(0, selected);
    }

    [Fact]
    public void LinearRankingPrefersBestAtLowThreshold()
    {
        var selector =
            new LinearRankingGeneticParentSelectionMethod<int>(2.0);

        int selected =
            selector.SelectParent(
                Population(10.0, 1.0, 5.0),
                OptimizationSense.Minimize,
                new ScriptedRandomSource(doubles: [0.0]));

        Assert.Equal(1, selected);
    }

    [Fact]
    public void LinearRankingSupportsMaximization()
    {
        var selector =
            new LinearRankingGeneticParentSelectionMethod<int>(2.0);

        int selected =
            selector.SelectParent(
                Population(10.0, 1.0, 5.0),
                OptimizationSense.Maximize,
                new ScriptedRandomSource(doubles: [0.0]));

        Assert.Equal(0, selected);
    }

    [Fact]
    public void ExponentialRankingUsesObjectiveRankNotRawMagnitude()
    {
        var selector =
            new ExponentialRankingGeneticParentSelectionMethod<int>(1.0);

        int selected =
            selector.SelectParent(
                Population(-1000000.0, -2.0, -1.0),
                OptimizationSense.Maximize,
                new ScriptedRandomSource(doubles: [0.0]));

        Assert.Equal(2, selected);
    }

    [Fact]
    public void ExplicitFitnessProportionateUsesSuppliedWeights()
    {
        var selector =
            new ExplicitFitnessProportionateGeneticParentSelectionMethod<int>(
                (member, sense) => member.Solution == 2 ? 10.0 : 0.0);

        int selected =
            selector.SelectParent(
                Population(100.0, -100.0, 0.0),
                OptimizationSense.Minimize,
                new ScriptedRandomSource(doubles: [0.9]));

        Assert.Equal(2, selected);
    }

    [Fact]
    public void ExplicitFitnessProportionateRejectsInvalidWeights()
    {
        var selector =
            new ExplicitFitnessProportionateGeneticParentSelectionMethod<int>(
                (member, sense) => -1.0);

        Assert.Throws<InvalidOperationException>(() =>
            selector.SelectParent(
                Population(1.0, 2.0),
                OptimizationSense.Minimize,
                new ScriptedRandomSource()));
    }

    [Fact]
    public void OnePointCrossoverExchangesSuffix()
    {
        var method = new OnePointGeneticCrossoverMethod<int>();

        GeneticOffspringPair<int[]> pair =
            method.Crossover(
                [1, 2, 3, 4],
                [9, 8, 7, 6],
                new ArrayProblem<int>(),
                new ScriptedRandomSource(ints: [1]));

        Assert.Equal(new[] {1, 2, 7, 6}, pair.First);
        Assert.Equal(new[] {9, 8, 3, 4}, pair.Second);
    }

    [Fact]
    public void TwoPointCrossoverExchangesSelectedSegment()
    {
        var method = new TwoPointGeneticCrossoverMethod<int>();

        GeneticOffspringPair<int[]> pair =
            method.Crossover(
                [1, 2, 3, 4, 5],
                [9, 8, 7, 6, 0],
                new ArrayProblem<int>(),
                new ScriptedRandomSource(ints: [1, 2]));

        Assert.Equal(new[] {1, 8, 7, 6, 5}, pair.First);
        Assert.Equal(new[] {9, 2, 3, 4, 0}, pair.Second);
    }

    [Fact]
    public void UniformCrossoverWithUnitProbabilitySwapsEveryLocus()
    {
        var method =
            new UniformGeneticCrossoverMethod<int>(1.0);

        GeneticOffspringPair<int[]> pair =
            method.Crossover(
                [1, 2, 3],
                [4, 5, 6],
                new ArrayProblem<int>(),
                new ScriptedRandomSource(doubles: [0.9, 0.9, 0.9]));

        Assert.Equal(new[] {4, 5, 6}, pair.First);
        Assert.Equal(new[] {1, 2, 3}, pair.Second);
    }

    [Fact]
    public void UniformCrossoverWithZeroProbabilityCopiesParents()
    {
        var method =
            new UniformGeneticCrossoverMethod<int>(0.0);

        GeneticOffspringPair<int[]> pair =
            method.Crossover(
                [1, 2],
                [3, 4],
                new ArrayProblem<int>(),
                new ScriptedRandomSource(doubles: [0.0, 0.0]));

        Assert.Equal(new[] {1, 2}, pair.First);
        Assert.Equal(new[] {3, 4}, pair.Second);
    }

    [Fact]
    public void SequenceCrossoverRejectsLengthMismatch()
    {
        var method = new OnePointGeneticCrossoverMethod<int>();

        Assert.Throws<ArgumentException>(() =>
            method.Crossover(
                [1, 2],
                [1],
                new ArrayProblem<int>(),
                new ScriptedRandomSource()));
    }

    [Fact]
    public void PmxPreservesPermutationAndCopiedSegment()
    {
        var method =
            new PartiallyMappedGeneticCrossoverMethod<int>();

        int[] first = [1, 2, 3, 4, 5, 6];
        int[] second = [4, 1, 2, 6, 5, 3];

        GeneticOffspringPair<int[]> pair =
            method.Crossover(
                first,
                second,
                new ArrayProblem<int>(),
                new ScriptedRandomSource(ints: [1, 2]));

        Assert.Equal(first.OrderBy(value => value).ToArray(), pair.First.OrderBy(value => value).ToArray());
        Assert.Equal(second.OrderBy(value => value).ToArray(), pair.Second.OrderBy(value => value).ToArray());
        Assert.Equal(first[1], pair.First[1]);
    }

    [Fact]
    public void PmxRejectsDuplicateAlleles()
    {
        var method =
            new PartiallyMappedGeneticCrossoverMethod<int>();

        Assert.Throws<ArgumentException>(() =>
            method.Crossover(
                [1, 1, 2],
                [1, 2, 3],
                new ArrayProblem<int>(),
                new ScriptedRandomSource()));
    }

    [Fact]
    public void PmxRejectsDifferentAlleleSets()
    {
        var method =
            new PartiallyMappedGeneticCrossoverMethod<int>();

        Assert.Throws<ArgumentException>(() =>
            method.Crossover(
                [1, 2, 3],
                [1, 2, 4],
                new ArrayProblem<int>(),
                new ScriptedRandomSource()));
    }

    [Fact]
    public void OrderCrossoverPreservesPermutationAndSegment()
    {
        var method =
            new OrderGeneticCrossoverMethod<int>();

        int[] first = [1, 2, 3, 4, 5, 6];
        int[] second = [4, 1, 2, 6, 5, 3];

        GeneticOffspringPair<int[]> pair =
            method.Crossover(
                first,
                second,
                new ArrayProblem<int>(),
                new ScriptedRandomSource(ints: [1, 2]));

        Assert.Equal(first.OrderBy(value => value).ToArray(), pair.First.OrderBy(value => value).ToArray());
        Assert.Equal(first[1], pair.First[1]);
        Assert.Equal(first[2], pair.First[2]);
    }

    [Fact]
    public void BoundedSbxKeepsChildrenInsideBounds()
    {
        var method =
            new BoundedSimulatedBinaryGeneticCrossoverMethod(
                [-1.0, -1.0],
                [1.0, 1.0],
                distributionIndex: 10.0,
                perVariableCrossoverProbability: 1.0);

        GeneticOffspringPair<double[]> pair =
            method.Crossover(
                [-0.9, -0.2],
                [0.9, 0.8],
                new DoubleArrayProblem(),
                new ScriptedRandomSource(
                    doubles: [0.0, 0.25, 0.75, 0.0, 0.5, 0.25]));

        Assert.All(pair.First, value => Assert.InRange(value, -1.0, 1.0));
        Assert.All(pair.Second, value => Assert.InRange(value, -1.0, 1.0));
    }

    [Fact]
    public void BoundedSbxLeavesEqualParentsEqual()
    {
        var method =
            new BoundedSimulatedBinaryGeneticCrossoverMethod(
                [0.0],
                [1.0],
                perVariableCrossoverProbability: 1.0);

        GeneticOffspringPair<double[]> pair =
            method.Crossover(
                [0.25],
                [0.25],
                new DoubleArrayProblem(),
                new ScriptedRandomSource(doubles: [0.0]));

        Assert.Equal(0.25, pair.First[0]);
        Assert.Equal(0.25, pair.Second[0]);
    }

    [Fact]
    public void BoundedSbxRejectsParentOutsideBounds()
    {
        var method =
            new BoundedSimulatedBinaryGeneticCrossoverMethod(
                [0.0],
                [1.0]);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            method.Crossover(
                [-0.1],
                [0.5],
                new DoubleArrayProblem(),
                new ScriptedRandomSource()));
    }

    [Fact]
    public void BitFlipMutationUsesPerBitProbability()
    {
        var method =
            new BitFlipGeneticMutationMethod(0.5);

        bool[] solution = [false, false, true, true];

        method.Mutate(
            solution,
            new BoolArrayProblem(),
            new ScriptedRandomSource(
                doubles: [0.1, 0.9, 0.2, 0.8]));

        Assert.Equal(new[] {true, false, false, true}, solution);
    }

    [Fact]
    public void IntegerRandomResetStaysInsideBoundsAndChangesSelectedGene()
    {
        var method =
            new IntegerRandomResetGeneticMutationMethod(
                [0, 10],
                [3, 13],
                perGeneProbability: 1.0);

        int[] solution = [1, 11];

        method.Mutate(
            solution,
            new IntArrayProblem(),
            new ScriptedRandomSource(
                doubles: [0.0, 0.0],
                ints: [0, 1]));

        Assert.InRange(solution[0], 0, 2);
        Assert.InRange(solution[1], 10, 12);
        Assert.NotEqual(1, solution[0]);
        Assert.NotEqual(11, solution[1]);
    }

    [Fact]
    public void IntegerRandomResetRejectsIntervalWiderThanRandomContract()
    {
        Assert.Throws<ArgumentException>(() =>
            new IntegerRandomResetGeneticMutationMethod(
                [int.MinValue],
                [int.MaxValue],
                perGeneProbability: 1.0));
    }

    [Fact]
    public void SwapMutationExchangesTwoDistinctPositions()
    {
        var method =
            new SwapGeneticMutationMethod<int>();

        int[] solution = [1, 2, 3, 4];

        method.Mutate(
            solution,
            new IntArrayProblem(),
            new ScriptedRandomSource(ints: [1, 1]));

        Assert.Equal(new[] {1, 3, 2, 4}, solution);
    }

    [Fact]
    public void InversionMutationReversesSelectedSegment()
    {
        var method =
            new InversionGeneticMutationMethod<int>();

        int[] solution = [1, 2, 3, 4, 5];

        method.Mutate(
            solution,
            new IntArrayProblem(),
            new ScriptedRandomSource(ints: [1, 2]));

        Assert.Equal(5, solution.Length);
        Assert.Equal(new[] {1, 4, 3, 2, 5}, solution);
    }

    [Fact]
    public void BoundedGaussianMutationProjectsToBounds()
    {
        var method =
            new BoundedGaussianGeneticMutationMethod(
                [0.0],
                [1.0],
                standardDeviation: 100.0,
                perGeneProbability: 1.0);

        double[] solution = [0.5];

        method.Mutate(
            solution,
            new DoubleArrayProblem(),
            new ScriptedRandomSource(
                doubles: [0.0, 0.5, 0.0]));

        Assert.InRange(solution[0], 0.0, 1.0);
    }

    [Fact]
    public void BoundedPolynomialMutationStaysInsideBounds()
    {
        var method =
            new BoundedPolynomialGeneticMutationMethod(
                [0.0, -2.0],
                [1.0, 2.0],
                distributionIndex: 20.0,
                perGeneProbability: 1.0);

        double[] solution = [0.5, 0.0];

        method.Mutate(
            solution,
            new DoubleArrayProblem(),
            new ScriptedRandomSource(
                doubles: [0.0, 0.1, 0.0, 0.9]));

        Assert.InRange(solution[0], 0.0, 1.0);
        Assert.InRange(solution[1], -2.0, 2.0);
    }

    [Fact]
    public void BoundedPolynomialMutationWithZeroProbabilityIsNoOp()
    {
        var method =
            new BoundedPolynomialGeneticMutationMethod(
                [0.0],
                [1.0],
                perGeneProbability: 0.0);

        double[] solution = [0.25];

        method.Mutate(
            solution,
            new DoubleArrayProblem(),
            new ScriptedRandomSource(doubles: [0.0]));

        Assert.Equal(0.25, solution[0]);
    }

    [Fact]
    public void RealMutationsRejectSolutionOutsideBounds()
    {
        var gaussian =
            new BoundedGaussianGeneticMutationMethod(
                [0.0],
                [1.0],
                standardDeviation: 1.0,
                perGeneProbability: 1.0);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            gaussian.Mutate(
                [2.0],
                new DoubleArrayProblem(),
                new ScriptedRandomSource()));
    }

    [Fact]
    public void AdvancedReferencesKeepVerifiedDoisAndExplicitNulls()
    {
        Assert.Equal(
            "10.1016/B978-0-08-050684-5.50008-2",
            AdvancedGeneticAlgorithmReferences.GoldbergDeb1991.Doi);

        Assert.Equal(
            "10.5555/645512.657265",
            AdvancedGeneticAlgorithmReferences.Syswerda1989.Doi);

        Assert.Equal(
            "10.1016/B978-0-08-050684-5.50009-4",
            AdvancedGeneticAlgorithmReferences.Syswerda1991.Doi);

        Assert.Equal(
            "10.5555/645511.657095",
            AdvancedGeneticAlgorithmReferences.GoldbergLingle1985.Doi);

        Assert.Equal(
            "10.5555/1625135.1625164",
            AdvancedGeneticAlgorithmReferences.Davis1985.Doi);

        Assert.Null(
            AdvancedGeneticAlgorithmReferences.DebAgrawal1995.Doi);

        Assert.Equal(
            "10.1109/4235.996017",
            AdvancedGeneticAlgorithmReferences.DebPratapAgarwalMeyarivan2002.Doi);

        Assert.Equal(
            "10.1504/IJAISC.2014.059280",
            AdvancedGeneticAlgorithmReferences.DebDeb2014.Doi);
    }

    private static GeneticPopulationMember<int>[] Population(
        params double[] objectives) =>
        objectives
            .Select(
                (objective, index) =>
                    new GeneticPopulationMember<int>(
                        index,
                        objective))
            .ToArray();

    private sealed class ArrayProblem<T> :
        IOptimizationProblem<T[]>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(T[] solution) => 0.0;
    }

    private sealed class IntArrayProblem :
        IOptimizationProblem<int[]>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(int[] solution) => solution.Sum();
    }

    private sealed class BoolArrayProblem :
        IOptimizationProblem<bool[]>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(bool[] solution) => solution.Count(value => value);
    }

    private sealed class DoubleArrayProblem :
        IOptimizationProblem<double[]>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(double[] solution) => solution.Sum();
    }

    private sealed class ScriptedRandomSource :
        IRandomSource
    {
        private readonly int[] _ints;
        private readonly double[] _doubles;
        private int _intIndex;
        private int _doubleIndex;

        public ScriptedRandomSource(
            int[]? ints = null,
            double[]? doubles = null)
        {
            _ints = ints ?? [0];
            _doubles = doubles ?? [0.0];
        }

        public ulong Seed => 0UL;

        public ulong NextUInt64() =>
            (ulong)NextInt32(int.MaxValue);

        public double NextDouble()
        {
            double value =
                _doubles[_doubleIndex % _doubles.Length];

            _doubleIndex++;

            if (value < 0.0 || value >= 1.0)
                throw new InvalidOperationException("Scripted double must be in [0,1).");

            return value;
        }

        public int NextInt32(
            int exclusiveMax)
        {
            if (exclusiveMax <= 0)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

            int raw =
                _ints[_intIndex % _ints.Length];

            _intIndex++;

            int value = raw % exclusiveMax;
            if (value < 0)
                value += exclusiveMax;

            return value;
        }

        public int NextInt32(
            int inclusiveMin,
            int exclusiveMax)
        {
            if (exclusiveMax <= inclusiveMin)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

            return inclusiveMin +
                NextInt32(exclusiveMax - inclusiveMin);
        }

        public void Fill(Span<byte> buffer) =>
            buffer.Clear();
    }
}
