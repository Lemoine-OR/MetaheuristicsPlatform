using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.WhaleOptimization;

public sealed class WhaleOptimizationAlgorithmOptimizer :
    IMetaheuristic<double[], WhaleOptimizationAlgorithmParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.WhaleOptimizationAlgorithm,
            Name = "Whale Optimization Algorithm",
            Acronym = "WOA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [WhaleOptimizationAlgorithmReferences.MirjaliliLewis2016]
        };

    public WhaleOptimizationAlgorithmParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        WhaleOptimizationAlgorithmParameters parameters,
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
            throw new NotSupportedException("WOA requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        int n = parameters.PopulationSize;

        if (dimension <= 0)
            throw new InvalidOperationException("WOA requires a positive dimension.");

        double[][] whales = CreatePopulation(n, dimension);
        double[][] next = CreatePopulation(n, dimension);
        double[] objectives = new double[n];

        var context = new OptimizationContext<double[]>(
            Descriptor,
            problem,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);

        var state = new WhaleOptimizationAlgorithmState(
            0,
            WhaleOptimizationAlgorithmPhase.Initialization,
            n,
            2.0,
            null);

        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, whales[i]);
            objectives[i] = context.Evaluate(whales[i], state);
            RequireFinite(objectives[i]);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int bestIndex = BestIndex(objectives, problem.Sense);
            double[] leader = (double[])whales[bestIndex].Clone();

            double sourceTime = iteration - 1.0;
            double a =
                2.0 -
                sourceTime *
                (2.0 / parameters.MaximumIterations);

            double a2 =
                -1.0 -
                sourceTime /
                parameters.MaximumIterations;

            state = new WhaleOptimizationAlgorithmState(
                iteration - 1,
                WhaleOptimizationAlgorithmPhase.Search,
                n,
                a,
                objectives[bestIndex]);

            for (int i = 0; i < n; i++)
            {
                double r1 = context.Random.NextDouble();
                double r2 = context.Random.NextDouble();
                double coefficientA = 2.0 * a * r1 - a;
                double coefficientC = 2.0 * r2;
                double l =
                    (a2 - 1.0) *
                    context.Random.NextDouble() +
                    1.0;
                double p = context.Random.NextDouble();

                for (int d = 0; d < dimension; d++)
                {
                    if (p < 0.5)
                    {
                        if (Math.Abs(coefficientA) >= 1.0)
                        {
                            int randomIndex = context.Random.NextInt32(n);
                            double randomLeader = whales[randomIndex][d];
                            double distance = Math.Abs(
                                coefficientC * randomLeader - whales[i][d]);

                            next[i][d] =
                                randomLeader -
                                coefficientA * distance;
                        }
                        else
                        {
                            double distance = Math.Abs(
                                coefficientC * leader[d] - whales[i][d]);

                            next[i][d] =
                                leader[d] -
                                coefficientA * distance;
                        }
                    }
                    else
                    {
                        double distance = Math.Abs(leader[d] - whales[i][d]);

                        next[i][d] =
                            distance *
                            Math.Exp(parameters.SpiralConstant * l) *
                            Math.Cos(2.0 * Math.PI * l) +
                            leader[d];
                    }
                }

                searchSpace.Clamp(next[i]);
                objectives[i] = context.Evaluate(next[i], state);
                RequireFinite(objectives[i]);

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            (whales, next) = (next, whales);
            bestIndex = BestIndex(objectives, problem.Sense);

            state = new WhaleOptimizationAlgorithmState(
                iteration,
                WhaleOptimizationAlgorithmPhase.CompletedIteration,
                n,
                a,
                objectives[bestIndex]);

            context.CompleteIteration(state.BestFitness, state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumWhaleOptimizationIterations",
                "The configured WOA iteration limit was reached."),
            state);
    }

    private static double[][] CreatePopulation(int count, int dimension)
    {
        double[][] population = new double[count][];
        for (int i = 0; i < count; i++)
            population[i] = new double[dimension];
        return population;
    }

    private static int BestIndex(ReadOnlySpan<double> values, OptimizationSense sense)
    {
        int best = 0;
        for (int i = 1; i < values.Length; i++)
        {
            if (sense.IsBetter(values[i], values[best]))
                best = i;
        }
        return best;
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
            throw new InvalidOperationException("WOA requires finite objective values.");
    }
}
