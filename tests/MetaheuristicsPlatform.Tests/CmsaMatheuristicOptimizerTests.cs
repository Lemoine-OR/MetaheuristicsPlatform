using MetaheuristicsPlatform.Algorithms.Matheuristics.CMSA;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class CmsaMatheuristicOptimizerTests
{
    [Fact]
    public void Optimize_UsesExactRepairDomain_AndFactoryCreatesCanonicalType()
    {
        TestDomain domain = new();

        MatheuristicOptimizationResult result =
            new CmsaMatheuristicOptimizer().Optimize(
                domain,
                new CmsaMatheuristicParameters(),
                new OptimizationOptions
                {
                    Seed = 77553311UL
                },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(double.IsFinite(result.BestObjective));
        Assert.True(result.Best.IsIntegerFeasible);
        Assert.NotEmpty(result.ExactRepairTrace);
        Assert.True(result.BestObjective <= 7.0);
        Assert.True(result.ExactSolves + result.RelaxationSolves > 0);

        Assert.IsType<CmsaMatheuristicOptimizer>(
            MetaheuristicFactory.Create<CmsaMatheuristicOptimizer>(
                MetaheuristicAlgorithmIds.CmsaMatheuristic));
    }


    private sealed class TestDomain :
        IExactRepairMatheuristicDomain
    {
        private static readonly MatheuristicVariableKind[] Kinds =
        {
            MatheuristicVariableKind.Binary,
            MatheuristicVariableKind.Binary,
            MatheuristicVariableKind.Binary,
            MatheuristicVariableKind.Binary
        };

        private static readonly double[] Weights =
        {
            4.0,
            1.0,
            3.0,
            2.0
        };

        public OptimizationSense Sense => OptimizationSense.Minimize;

        public IReadOnlyList<MatheuristicVariableKind> VariableKinds => Kinds;

        public MatheuristicPoint CreateInitial(
            IRandomSource random)
        {
            int choice = random.NextInt32(4);

            double[] values =
                choice switch
                {
                    0 => new[] { 1.0, 0.0, 1.0, 0.0 },
                    1 => new[] { 1.0, 0.0, 0.0, 1.0 },
                    2 => new[] { 0.0, 1.0, 1.0, 0.0 },
                    _ => new[] { 0.0, 1.0, 0.0, 1.0 }
                };

            return new MatheuristicPoint(
                values,
                Evaluate(values),
                true);
        }

        public double Evaluate(
            IReadOnlyList<double> values)
        {
            double total = 0.0;

            for (int index = 0; index < Weights.Length; index++)
                total +=
                    Weights[index] *
                    values[index];

            return total;
        }

        public bool IsIntegerFeasible(
            IReadOnlyList<double> values)
        {
            if (values.Count != Kinds.Length)
                return false;

            int selected = 0;

            for (int index = 0; index < values.Count; index++)
            {
                double rounded =
                    Math.Round(
                        values[index],
                        MidpointRounding.AwayFromZero);

                if (Math.Abs(
                        values[index] -
                        rounded) > 1e-9 ||
                    (rounded != 0.0 &&
                     rounded != 1.0))
                    return false;

                if (rounded == 1.0)
                    selected++;
            }

            return selected == 2;
        }

        public MatheuristicSolveResult SolveRelaxation(
            ExactRepairRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double[] values =
            {
                0.15,
                0.85,
                0.25,
                0.75
            };

            if (request.TargetValues is not null)
            {
                for (int index = 0; index < values.Length; index++)
                    values[index] =
                        0.8 *
                        request.TargetValues[index] +
                        0.2 *
                        values[index];
            }

            HashSet<int>? allowed =
                request.AllowedActiveIndices is null
                    ? null
                    : new HashSet<int>(
                        request.AllowedActiveIndices);

            if (allowed is not null)
            {
                for (int index = 0; index < values.Length; index++)
                    if (!allowed.Contains(index))
                        values[index] = 0.0;
            }

            foreach (KeyValuePair<int, double> fixedValue in
                request.FixedValues)
                values[fixedValue.Key] =
                    fixedValue.Value;

            foreach (KeyValuePair<int, MatheuristicVariableBound> bound in
                request.Bounds)
                values[bound.Key] =
                    Math.Clamp(
                        values[bound.Key],
                        bound.Value.Lower,
                        bound.Value.Upper);

            double[] reducedCosts =
            {
                2.0,
                -1.0,
                1.0,
                -0.5
            };

            return MatheuristicSolveResult.FromPoint(
                new MatheuristicPoint(
                    values,
                    Evaluate(values),
                    IsIntegerFeasible(values),
                    reducedCosts),
                exploredNodes: 1);
        }

        public MatheuristicSolveResult SolveExact(
            ExactRepairRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HashSet<int>? allowed =
                request.AllowedActiveIndices is null
                    ? null
                    : new HashSet<int>(
                        request.AllowedActiveIndices);

            MatheuristicPoint? best = null;
            double bestKey =
                double.PositiveInfinity;

            for (int mask = 0; mask < 16; mask++)
            {
                double[] values =
                    new double[4];

                for (int index = 0; index < values.Length; index++)
                    values[index] =
                        (mask & (1 << index)) != 0
                            ? 1.0
                            : 0.0;

                if (!IsIntegerFeasible(values))
                    continue;

                if (allowed is not null &&
                    Enumerable.Range(0, values.Length)
                        .Any(
                            index =>
                                values[index] >= 0.5 &&
                                !allowed.Contains(index)))
                    continue;

                bool rejected = false;

                foreach (KeyValuePair<int, double> fixedValue in
                    request.FixedValues)
                    if (Math.Abs(
                            values[fixedValue.Key] -
                            fixedValue.Value) > 1e-9)
                    {
                        rejected = true;
                        break;
                    }

                if (rejected)
                    continue;

                foreach (KeyValuePair<int, MatheuristicVariableBound> bound in
                    request.Bounds)
                    if (values[bound.Key] <
                            bound.Value.Lower - 1e-9 ||
                        values[bound.Key] >
                            bound.Value.Upper + 1e-9)
                    {
                        rejected = true;
                        break;
                    }

                if (rejected)
                    continue;

                if (request.HammingRadius is not null &&
                    request.ReferenceValues is not null &&
                    Hamming(
                        values,
                        request.ReferenceValues) >
                        request.HammingRadius.Value)
                    continue;

                if (request.DistanceLimit is not null &&
                    request.ReferenceValues is not null &&
                    Distance(
                        values,
                        request.ReferenceValues) >
                        request.DistanceLimit.Value + 1e-9)
                    continue;

                double objective =
                    Evaluate(values);

                if (request.ObjectiveCutoff is not null &&
                    objective >
                        request.ObjectiveCutoff.Value + 1e-9)
                    continue;

                double key =
                    request.Mode switch
                    {
                        MatheuristicSolveMode.DistanceToTarget
                            when request.TargetValues is not null =>
                            Distance(
                                values,
                                request.TargetValues),

                        MatheuristicSolveMode.WeightedDistanceAndObjective
                            when request.TargetValues is not null =>
                            (1.0 - request.OriginalObjectiveWeight) *
                            Distance(
                                values,
                                request.TargetValues) +
                            request.OriginalObjectiveWeight *
                            objective,

                        MatheuristicSolveMode.ProximityToReference
                            when request.ReferenceValues is not null =>
                            Hamming(
                                values,
                                request.ReferenceValues),

                        _ => objective
                    };

                if (key < bestKey - 1e-12 ||
                    (Math.Abs(key - bestKey) <= 1e-12 &&
                     (best is null ||
                      objective < best.Objective)))
                {
                    bestKey = key;
                    best =
                        new MatheuristicPoint(
                            values,
                            objective,
                            true);
                }
            }

            return best is null
                ? MatheuristicSolveResult.NoSolution(16)
                : MatheuristicSolveResult.FromPoint(
                    best,
                    16);
        }

        private static int Hamming(
            IReadOnlyList<double> first,
            IReadOnlyList<double> second)
        {
            int distance = 0;

            for (int index = 0; index < first.Count; index++)
                if ((first[index] >= 0.5) !=
                    (second[index] >= 0.5))
                    distance++;

            return distance;
        }

        private static double Distance(
            IReadOnlyList<double> first,
            IReadOnlyList<double> second)
        {
            double distance = 0.0;

            for (int index = 0; index < first.Count; index++)
                distance +=
                    Math.Abs(
                        first[index] -
                        second[index]);

            return distance;
        }
    }

}
