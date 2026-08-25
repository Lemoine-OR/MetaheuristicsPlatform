using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.MothFlame;

public sealed class MothFlameOptimizer :
    IMetaheuristic<double[], MothFlameOptimizerParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.MothFlameOptimization,
            Name = "Moth-Flame Optimization",
            Acronym = "MFO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [MothFlameOptimizerReferences.Mirjalili2015]
        };

    public MothFlameOptimizerParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        MothFlameOptimizerParameters parameters,
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
                "MFO requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension = searchSpace.Dimension;
        int n = parameters.PopulationSize;

        if (dimension <= 0)
            throw new InvalidOperationException("MFO requires a positive dimension.");

        double[][] moths = CreatePopulation(n, dimension);
        double[][] nextMoths = CreatePopulation(n, dimension);
        double[][] flames = CreatePopulation(n, dimension);
        double[][] merged = CreatePopulation(2 * n, dimension);
        double[] objectives = new double[n];
        double[] flameObjectives = new double[n];
        double[] mergedObjectives = new double[2 * n];

        var context = new OptimizationContext<double[]>(
            Descriptor,
            problem,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);

        var state = new MothFlameOptimizerState(
            0,
            MothFlameOptimizerPhase.Initialization,
            n,
            n,
            -1.0,
            null);

        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, moths[i]);
            objectives[i] = context.Evaluate(moths[i], state);
            RequireFinite(objectives[i]);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        InitializeFlames(
            moths,
            objectives,
            flames,
            flameObjectives,
            problem.Sense,
            dimension);

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int flameCount = Math.Clamp(
                (int)Math.Round(
                    n -
                    iteration *
                    ((n - 1.0) / parameters.MaximumIterations),
                    MidpointRounding.AwayFromZero),
                1,
                n);

            double a =
                -1.0 -
                (double)iteration /
                parameters.MaximumIterations;

            state = new MothFlameOptimizerState(
                iteration - 1,
                MothFlameOptimizerPhase.Search,
                n,
                flameCount,
                a,
                flameObjectives[0]);

            for (int i = 0; i < n; i++)
            {
                int flameIndex =
                    Math.Min(i, flameCount - 1);

                for (int d = 0; d < dimension; d++)
                {
                    double distance =
                        Math.Abs(
                            flames[flameIndex][d] -
                            moths[i][d]);

                    double tau =
                        (a - 1.0) *
                        context.Random.NextDouble() +
                        1.0;

                    nextMoths[i][d] =
                        distance *
                        Math.Exp(parameters.SpiralConstant * tau) *
                        Math.Cos(2.0 * Math.PI * tau) +
                        flames[flameIndex][d];
                }

                searchSpace.Clamp(nextMoths[i]);

                objectives[i] =
                    context.Evaluate(nextMoths[i], state);

                RequireFinite(objectives[i]);

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            MergeAndSelectFlames(
                nextMoths,
                objectives,
                flames,
                flameObjectives,
                merged,
                mergedObjectives,
                problem.Sense,
                dimension);

            (moths, nextMoths) =
                (nextMoths, moths);

            state = new MothFlameOptimizerState(
                iteration,
                MothFlameOptimizerPhase.CompletedIteration,
                n,
                flameCount,
                a,
                flameObjectives[0]);

            context.CompleteIteration(
                state.BestFlameFitness,
                state);

            StoppingDecision iterationStop =
                context.EvaluateStopping(state);

            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumMothFlameIterations",
                "The configured MFO iteration limit was reached."),
            state);
    }

    private static double[][] CreatePopulation(int count, int dimension)
    {
        double[][] population = new double[count][];
        for (int i = 0; i < count; i++)
            population[i] = new double[dimension];
        return population;
    }

    private static void InitializeFlames(
        double[][] moths,
        double[] objectives,
        double[][] flames,
        double[] flameObjectives,
        OptimizationSense sense,
        int dimension)
    {
        int[] order = RankBestFirst(objectives, sense);

        for (int rank = 0; rank < order.Length; rank++)
        {
            int source = order[rank];
            Array.Copy(moths[source], flames[rank], dimension);
            flameObjectives[rank] = objectives[source];
        }
    }

    private static void MergeAndSelectFlames(
        double[][] moths,
        double[] objectives,
        double[][] flames,
        double[] flameObjectives,
        double[][] merged,
        double[] mergedObjectives,
        OptimizationSense sense,
        int dimension)
    {
        int n = moths.Length;

        for (int i = 0; i < n; i++)
        {
            Array.Copy(moths[i], merged[i], dimension);
            mergedObjectives[i] = objectives[i];
            Array.Copy(flames[i], merged[n + i], dimension);
            mergedObjectives[n + i] = flameObjectives[i];
        }

        int[] order = RankBestFirst(mergedObjectives, sense);

        for (int rank = 0; rank < n; rank++)
        {
            int source = order[rank];
            Array.Copy(merged[source], flames[rank], dimension);
            flameObjectives[rank] = mergedObjectives[source];
        }
    }

    private static int[] RankBestFirst(
        ReadOnlySpan<double> values,
        OptimizationSense sense)
    {
        int[] order = Enumerable.Range(0, values.Length).ToArray();
        double[] snapshot = values.ToArray();

        Array.Sort(
            order,
            (left, right) =>
            {
                if (snapshot[left] == snapshot[right])
                    return left.CompareTo(right);

                return sense.IsBetter(snapshot[left], snapshot[right])
                    ? -1
                    : 1;
            });

        return order;
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
            throw new InvalidOperationException("MFO requires finite objective values.");
    }
}
