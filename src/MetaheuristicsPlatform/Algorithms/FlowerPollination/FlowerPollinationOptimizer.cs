using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.FlowerPollination;

public sealed class FlowerPollinationOptimizer :
    IMetaheuristic<double[], FlowerPollinationParameters>
{
    // Fixed numerical realization of the published Levy-flight operator.
    private const double MantegnaSigmaBeta15 =
        0.6965745025576968;

    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.FlowerPollinationAlgorithm,
            Name = "Flower Pollination Algorithm",
            Acronym = "FPA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [FlowerPollinationReferences.Yang2012]
        };

    public FlowerPollinationParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        FlowerPollinationParameters parameters,
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
                "FPA requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
            throw new InvalidOperationException(
                "FPA requires a positive dimension.");

        int n =
            parameters.PopulationSize;

        double[][] flowers =
            new double[n][];

        double[] objectives =
            new double[n];

        for (int i = 0; i < n; i++)
            flowers[i] = new double[dimension];

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
            new FlowerPollinationState(
                0,
                FlowerPollinationPhase.Initialization,
                n,
                0,
                0,
                null);

        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(
                context.Random,
                flowers[i]);

            objectives[i] =
                context.Evaluate(
                    flowers[i],
                    state);

            RequireFinite(objectives[i]);

            StoppingDecision stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        int globalCount = 0;
        int localCount = 0;

        double[] candidate =
            new double[dimension];

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int bestIndex =
                BestIndex(
                    objectives,
                    problem.Sense);

            double[] best =
                (double[])flowers[bestIndex].Clone();

            state =
                new FlowerPollinationState(
                    iteration - 1,
                    FlowerPollinationPhase.Search,
                    n,
                    globalCount,
                    localCount,
                    objectives[bestIndex]);

            for (int i = 0; i < n; i++)
            {
                if (context.Random.NextDouble() <
                    parameters.GlobalPollinationProbability)
                {
                    for (int d = 0; d < dimension; d++)
                    {
                        double levy =
                            parameters.LevyScale *
                            LevyStep(context.Random);

                        candidate[d] =
                            flowers[i][d] +
                            levy *
                            (
                                best[d] -
                                flowers[i][d]
                            );
                    }

                    globalCount++;
                }
                else
                {
                    int first =
                        context.Random.NextInt32(n);

                    int second;

                    do
                    {
                        second =
                            context.Random.NextInt32(n);
                    }
                    while (second == first);

                    double epsilon =
                        context.Random.NextDouble();

                    for (int d = 0; d < dimension; d++)
                    {
                        candidate[d] =
                            flowers[i][d] +
                            epsilon *
                            (
                                flowers[first][d] -
                                flowers[second][d]
                            );
                    }

                    localCount++;
                }

                searchSpace.Clamp(
                    candidate.AsSpan());

                double candidateObjective =
                    context.Evaluate(
                        candidate,
                        state);

                RequireFinite(candidateObjective);

                if (problem.Sense.IsBetter(
                        candidateObjective,
                        objectives[i]))
                {
                    Array.Copy(
                        candidate,
                        flowers[i],
                        dimension);

                    objectives[i] =
                        candidateObjective;
                }

                state =
                    state with
                    {
                        GlobalPollinations = globalCount,
                        LocalPollinations = localCount
                    };

                StoppingDecision stop =
                    context.EvaluateStopping(state);

                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            bestIndex =
                BestIndex(
                    objectives,
                    problem.Sense);

            state =
                new FlowerPollinationState(
                    iteration,
                    FlowerPollinationPhase.CompletedIteration,
                    n,
                    globalCount,
                    localCount,
                    objectives[bestIndex]);

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
                "MaximumFlowerPollinationIterations",
                "The configured FPA iteration limit was reached."),
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

    private static int BestIndex(
        ReadOnlySpan<double> values,
        OptimizationSense sense)
    {
        int best = 0;

        for (int i = 1; i < values.Length; i++)
        {
            if (sense.IsBetter(
                    values[i],
                    values[best]))
            {
                best = i;
            }
        }

        return best;
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new InvalidOperationException(
                "FPA requires finite objective values.");
        }
    }
}
