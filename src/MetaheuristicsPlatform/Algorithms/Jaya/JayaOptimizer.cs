using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Jaya;

public sealed class JayaOptimizer :
    IMetaheuristic<double[], JayaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.Jaya,
            Name = "Jaya Algorithm",
            Acronym = "Jaya",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [JayaReferences.Rao2016]
        };

    public JayaParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        JayaParameters parameters,
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
            throw new NotSupportedException("Jaya requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        if (dimension <= 0)
            throw new InvalidOperationException("Jaya requires a positive dimension.");

        int n = parameters.PopulationSize;
        double[][] population = CreatePopulation(n, dimension);
        double[] objectives = new double[n];
        double[] best = new double[dimension];
        double[] worst = new double[dimension];
        double[] candidate = new double[dimension];

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new JayaState(0, JayaPhase.Initialization, n, null, null);
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
            int worstIndex = WorstIndex(objectives, problem.Sense);
            Array.Copy(population[bestIndex], best, dimension);
            Array.Copy(population[worstIndex], worst, dimension);

            state = new JayaState(
                iteration - 1,
                JayaPhase.Search,
                n,
                objectives[bestIndex],
                objectives[worstIndex]);

            for (int i = 0; i < n; i++)
            {
                for (int d = 0; d < dimension; d++)
                {
                    double r1 = context.Random.NextDouble();
                    double r2 = context.Random.NextDouble();
                    double magnitude = Math.Abs(population[i][d]);
                    candidate[d] =
                        population[i][d] +
                        r1 * (best[d] - magnitude) -
                        r2 * (worst[d] - magnitude);
                }

                searchSpace.Clamp(candidate);
                double candidateObjective = context.Evaluate(candidate, state);
                RequireFinite(candidateObjective);

                if (problem.Sense.IsBetter(candidateObjective, objectives[i]))
                {
                    Array.Copy(candidate, population[i], dimension);
                    objectives[i] = candidateObjective;
                }

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            bestIndex = BestIndex(objectives, problem.Sense);
            worstIndex = WorstIndex(objectives, problem.Sense);
            state = new JayaState(
                iteration,
                JayaPhase.CompletedIteration,
                n,
                objectives[bestIndex],
                objectives[worstIndex]);

            context.CompleteIteration(state.BestFitness, state);
            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumJayaIterations",
                "The configured Jaya iteration limit was reached."),
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
        int index = 0;
        for (int i = 1; i < values.Length; i++)
            if (sense.IsBetter(values[i], values[index])) index = i;
        return index;
    }

    private static int WorstIndex(ReadOnlySpan<double> values, OptimizationSense sense)
    {
        int index = 0;
        for (int i = 1; i < values.Length; i++)
            if (sense.IsBetter(values[index], values[i])) index = i;
        return index;
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
            throw new InvalidOperationException("Jaya requires finite objective values.");
    }
}
