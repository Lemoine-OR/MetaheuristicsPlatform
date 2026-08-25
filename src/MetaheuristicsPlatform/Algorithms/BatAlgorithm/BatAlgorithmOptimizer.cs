using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.BatAlgorithm;

public sealed class BatAlgorithmOptimizer :
    IMetaheuristic<double[], BatAlgorithmParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.BatAlgorithm,
            Name = "Bat Algorithm",
            Acronym = "BA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms =
                MetaheuristicMechanism.Swarm |
                MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [BatAlgorithmReferences.Yang2010]
        };

    public BatAlgorithmParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        BatAlgorithmParameters parameters,
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
                "Bat Algorithm requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
            throw new InvalidOperationException(
                "Bat Algorithm requires a positive dimension.");

        int n =
            parameters.PopulationSize;

        double[][] positions =
            new double[n][];

        double[][] velocities =
            new double[n][];

        double[] objectives =
            new double[n];

        double[] loudness =
            Enumerable.Repeat(
                parameters.InitialLoudness,
                n).ToArray();

        double[] pulseRates =
            Enumerable.Repeat(
                parameters.InitialPulseRate,
                n).ToArray();

        for (int i = 0; i < n; i++)
        {
            positions[i] =
                new double[dimension];

            velocities[i] =
                new double[dimension];
        }

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
            new BatAlgorithmState(
                0,
                BatAlgorithmPhase.Initialization,
                n,
                0,
                parameters.InitialLoudness,
                null);

        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(
                context.Random,
                positions[i]);

            objectives[i] =
                context.Evaluate(
                    positions[i],
                    state);

            RequireFinite(objectives[i]);

            StoppingDecision stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        int acceptedMoves = 0;

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
                (double[])positions[bestIndex].Clone();

            double bestObjective =
                objectives[bestIndex];

            double meanLoudness =
                loudness.Average();

            state =
                new BatAlgorithmState(
                    iteration - 1,
                    BatAlgorithmPhase.Search,
                    n,
                    acceptedMoves,
                    meanLoudness,
                    objectives[bestIndex]);

            for (int i = 0; i < n; i++)
            {
                double frequency =
                    parameters.MinimumFrequency +
                    (
                        parameters.MaximumFrequency -
                        parameters.MinimumFrequency
                    ) *
                    context.Random.NextDouble();

                for (int d = 0; d < dimension; d++)
                {
                    velocities[i][d] +=
                        (
                            positions[i][d] -
                            best[d]
                        ) *
                        frequency;

                    candidate[d] =
                        positions[i][d] +
                        velocities[i][d];
                }

                if (context.Random.NextDouble() >
                    pulseRates[i])
                {
                    double epsilon =
                        (
                            2.0 *
                            context.Random.NextDouble()
                        ) -
                        1.0;

                    for (int d = 0; d < dimension; d++)
                    {
                        candidate[d] =
                            best[d] +
                            epsilon *
                            meanLoudness;
                    }
                }

                searchSpace.Clamp(
                    candidate.AsSpan());

                double candidateObjective =
                    context.Evaluate(
                        candidate,
                        state);

                RequireFinite(candidateObjective);

                bool improvesGlobalBest =
                    problem.Sense.IsBetter(
                        candidateObjective,
                        bestObjective);

                if (context.Random.NextDouble() <
                        loudness[i] &&
                    improvesGlobalBest)
                {
                    Array.Copy(
                        candidate,
                        positions[i],
                        dimension);

                    objectives[i] =
                        candidateObjective;

                    bestObjective =
                        candidateObjective;

                    loudness[i] *=
                        parameters.LoudnessDecay;

                    pulseRates[i] =
                        parameters.InitialPulseRate *
                        (
                            1.0 -
                            Math.Exp(
                                -parameters.PulseGrowth *
                                iteration)
                        );

                    acceptedMoves++;
                }

                state =
                    state with
                    {
                        AcceptedMoves = acceptedMoves,
                        MeanLoudness = loudness.Average()
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
                new BatAlgorithmState(
                    iteration,
                    BatAlgorithmPhase.CompletedIteration,
                    n,
                    acceptedMoves,
                    loudness.Average(),
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
                "MaximumBatAlgorithmIterations",
                "The configured Bat Algorithm iteration limit was reached."),
            state);
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
                "Bat Algorithm requires finite objective values.");
        }
    }
}
