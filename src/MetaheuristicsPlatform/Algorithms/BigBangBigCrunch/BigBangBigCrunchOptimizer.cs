using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.BigBangBigCrunch;

public sealed class BigBangBigCrunchOptimizer :
    IMetaheuristic<double[], BigBangBigCrunchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.BigBangBigCrunch,
            Name = "Big Bang-Big Crunch",
            Acronym = "BB-BC",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [BigBangBigCrunchReferences.ErolEksin2006]
        };

    public BigBangBigCrunchParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        BigBangBigCrunchParameters parameters,
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
            throw new NotSupportedException("BB-BC requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        if (dimension <= 0)
            throw new InvalidOperationException("BB-BC requires a positive dimension.");

        int n = parameters.PopulationSize;
        double[][] population = CreatePopulation(n, dimension);
        double[][] nextPopulation = CreatePopulation(n, dimension);
        double[] objectives = new double[n];
        double[] nextObjectives = new double[n];
        double[] representative = new double[dimension];

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new BigBangBigCrunchState(
            0, BigBangBigCrunchPhase.Initialization, n, 1.0, null);
        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, population[i]);
            objectives[i] = context.Evaluate(population[i], state);
            RequireFinite(objectives[i]);
            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int bestIndex = BestIndex(objectives, problem.Sense);
            Array.Copy(population[bestIndex], representative, dimension);
            double radiusFactor = parameters.Alpha / iteration;

            state = new BigBangBigCrunchState(
                iteration - 1,
                BigBangBigCrunchPhase.BigBang,
                n,
                radiusFactor,
                objectives[bestIndex]);

            ReadOnlySpan<double> lower = searchSpace.LowerBounds;
            ReadOnlySpan<double> upper = searchSpace.UpperBounds;

            for (int i = 0; i < n; i++)
            {
                for (int d = 0; d < dimension; d++)
                {
                    double z = NextStandardNormal(context.Random.NextDouble(), context.Random.NextDouble());
                    nextPopulation[i][d] =
                        representative[d] +
                        z * radiusFactor * (upper[d] - lower[d]);
                }

                searchSpace.Clamp(nextPopulation[i]);
                nextObjectives[i] = context.Evaluate(nextPopulation[i], state);
                RequireFinite(nextObjectives[i]);

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            (population, nextPopulation) = (nextPopulation, population);
            (objectives, nextObjectives) = (nextObjectives, objectives);

            int currentBest = BestIndex(objectives, problem.Sense);
            state = new BigBangBigCrunchState(
                iteration,
                BigBangBigCrunchPhase.CompletedIteration,
                n,
                radiusFactor,
                objectives[currentBest]);

            context.CompleteIteration(state.RepresentativeFitness, state);
            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumBigBangBigCrunchIterations",
                "The configured BB-BC iteration limit was reached."),
            state);
    }

    private static double[][] CreatePopulation(int count, int dimension)
    {
        double[][] result = new double[count][];
        for (int i = 0; i < count; i++) result[i] = new double[dimension];
        return result;
    }

    private static int BestIndex(ReadOnlySpan<double> values, OptimizationSense sense)
    {
        int best = 0;
        for (int i = 1; i < values.Length; i++)
            if (sense.IsBetter(values[i], values[best])) best = i;
        return best;
    }

    private static double NextStandardNormal(double u1Raw, double u2)
    {
        double u1 = 1.0 - u1Raw;
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
            throw new InvalidOperationException("BB-BC requires finite objective values.");
    }
}
