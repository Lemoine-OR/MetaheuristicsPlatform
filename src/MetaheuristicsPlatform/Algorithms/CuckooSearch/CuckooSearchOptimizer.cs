using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.CuckooSearch;

public sealed class CuckooSearchOptimizer :
    IMetaheuristic<double[], CuckooSearchParameters>
{
    // Fixed numerical realization of the published Levy-flight operator.
    private const double MantegnaSigmaBeta15 =
        0.6965745025576968;

    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.CuckooSearch,
            Name = "Cuckoo Search via Levy Flights",
            Acronym = "CS",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [CuckooSearchReferences.YangDeb2009]
        };

    public CuckooSearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        CuckooSearchParameters parameters,
        ISolutionCloner<double[]> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<double[]>? callback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(solutionCloner);
        ArgumentNullException.ThrowIfNull(stoppingCriterion);
        parameters.Validate();

        if (problem is not ISpanContinuousOptimizationProblem continuousProblem)
            throw new NotSupportedException(
                "Cuckoo Search requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
            throw new InvalidOperationException(
                "Cuckoo Search requires a positive dimension.");

        int n =
            parameters.NestCount;

        double[][] nests =
            new double[n][];

        double[] objectives =
            new double[n];

        for (int i = 0; i < n; i++)
            nests[i] = new double[dimension];

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        var state =
            new CuckooSearchState(
                0,
                CuckooSearchPhase.Initialization,
                n,
                0,
                0,
                null);

        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(
                context.Random,
                nests[i]);

            objectives[i] =
                context.Evaluate(
                    nests[i],
                    state);

            RequireFinite(objectives[i]);

            StoppingDecision stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        int levyFlights = 0;
        int abandonedNests = 0;

        double[] candidate =
            new double[dimension];

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            state =
                new CuckooSearchState(
                    iteration - 1,
                    CuckooSearchPhase.Search,
                    n,
                    levyFlights,
                    abandonedNests,
                    BestObjective(objectives, problem.Sense));

            int cuckoo =
                context.Random.NextInt32(n);

            for (int d = 0; d < dimension; d++)
            {
                candidate[d] =
                    nests[cuckoo][d] +
                    parameters.LevyScale *
                    LevyStep(context.Random);
            }

            searchSpace.Clamp(
                candidate.AsSpan());

            double candidateObjective =
                context.Evaluate(
                    candidate,
                    state);

            RequireFinite(candidateObjective);

            levyFlights++;

            int target =
                context.Random.NextInt32(n);

            if (problem.Sense.IsBetter(
                    candidateObjective,
                    objectives[target]))
            {
                Array.Copy(
                    candidate,
                    nests[target],
                    dimension);

                objectives[target] =
                    candidateObjective;
            }

            state =
                state with
                {
                    LevyFlights = levyFlights
                };

            StoppingDecision cuckooStop =
                context.EvaluateStopping(state);

            if (cuckooStop.ShouldStop)
                return context.Complete(cuckooStop, state);

            int abandonCount =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        parameters.DiscoveryProbability * n));

            int[] worstFirst =
                RankWorstFirst(
                    objectives,
                    problem.Sense);

            for (int k = 0; k < abandonCount; k++)
            {
                int index =
                    worstFirst[k];

                searchSpace.Sample(
                    context.Random,
                    nests[index]);

                objectives[index] =
                    context.Evaluate(
                        nests[index],
                        state);

                RequireFinite(objectives[index]);

                abandonedNests++;

                state =
                    state with
                    {
                        AbandonedNests = abandonedNests
                    };

                StoppingDecision stop =
                    context.EvaluateStopping(state);

                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            state =
                new CuckooSearchState(
                    iteration,
                    CuckooSearchPhase.CompletedIteration,
                    n,
                    levyFlights,
                    abandonedNests,
                    BestObjective(objectives, problem.Sense));

            context.CompleteIteration(
                state.IterationBestFitness,
                state);

            StoppingDecision iterationStop =
                context.EvaluateStopping(state);

            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumCuckooSearchIterations",
                "The configured Cuckoo Search iteration limit was reached."),
            state);
    }

    private static double LevyStep(
        IRandomSource random)
    {
        double u =
            MantegnaSigmaBeta15 *
            StandardNormal(random);

        double v =
            StandardNormal(random);

        double denominator =
            Math.Pow(
                Math.Abs(v),
                2.0 / 3.0);

        if (denominator < 1e-15)
            denominator = 1e-15;

        return u / denominator;
    }

    private static double StandardNormal(
        IRandomSource random)
    {
        double u1 =
            Math.Max(
                random.NextDouble(),
                1e-15);

        double u2 =
            random.NextDouble();

        return
            Math.Sqrt(
                -2.0 * Math.Log(u1)) *
            Math.Cos(
                2.0 * Math.PI * u2);
    }

    private static int[] RankWorstFirst(
        ReadOnlySpan<double> values,
        OptimizationSense sense)
    {
        int[] order =
            Enumerable.Range(
                0,
                values.Length).ToArray();

        double[] snapshot =
            values.ToArray();

        Array.Sort(
            order,
            (left, right) =>
            {
                if (snapshot[left] == snapshot[right])
                    return left.CompareTo(right);

                return sense.IsBetter(
                    snapshot[left],
                    snapshot[right])
                    ? 1
                    : -1;
            });

        return order;
    }

    private static double BestObjective(
        ReadOnlySpan<double> values,
        OptimizationSense sense)
    {
        double best =
            values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (sense.IsBetter(values[i], best))
                best = values[i];
        }

        return best;
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new InvalidOperationException(
                "Cuckoo Search requires finite objective values.");
        }
    }
}
