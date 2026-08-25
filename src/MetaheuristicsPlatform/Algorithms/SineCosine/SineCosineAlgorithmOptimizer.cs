using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.SineCosine;

public sealed class SineCosineAlgorithmOptimizer :
    IMetaheuristic<double[], SineCosineAlgorithmParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.SineCosineAlgorithm,
            Name = "Sine Cosine Algorithm",
            Acronym = "SCA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [SineCosineAlgorithmReferences.Mirjalili2016]
        };

    public SineCosineAlgorithmParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        SineCosineAlgorithmParameters parameters,
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
            throw new NotSupportedException("SCA requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        int n = parameters.PopulationSize;

        if (dimension <= 0)
            throw new InvalidOperationException("SCA requires a positive dimension.");

        double[][] agents = CreatePopulation(n, dimension);
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

        var state = new SineCosineAlgorithmState(
            0,
            SineCosineAlgorithmPhase.Initialization,
            n,
            parameters.InitialAmplitude,
            null);

        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, agents[i]);
            objectives[i] = context.Evaluate(agents[i], state);
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
            double[] destination = (double[])agents[bestIndex].Clone();

            double sourceTime = iteration - 1.0;
            double r1 =
                parameters.InitialAmplitude *
                (1.0 - sourceTime / parameters.MaximumIterations);

            state = new SineCosineAlgorithmState(
                iteration - 1,
                SineCosineAlgorithmPhase.Search,
                n,
                r1,
                objectives[bestIndex]);

            for (int i = 0; i < n; i++)
            {
                for (int d = 0; d < dimension; d++)
                {
                    double r2 = 2.0 * Math.PI * context.Random.NextDouble();
                    double r3 = 2.0 * context.Random.NextDouble();
                    double r4 = context.Random.NextDouble();
                    double distance = Math.Abs(
                        r3 * destination[d] - agents[i][d]);

                    double oscillation =
                        r4 < 0.5
                            ? Math.Sin(r2)
                            : Math.Cos(r2);

                    next[i][d] =
                        agents[i][d] +
                        r1 * oscillation * distance;
                }

                searchSpace.Clamp(next[i]);
                objectives[i] = context.Evaluate(next[i], state);
                RequireFinite(objectives[i]);

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            (agents, next) = (next, agents);
            bestIndex = BestIndex(objectives, problem.Sense);

            state = new SineCosineAlgorithmState(
                iteration,
                SineCosineAlgorithmPhase.CompletedIteration,
                n,
                r1,
                objectives[bestIndex]);

            context.CompleteIteration(state.BestFitness, state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumSineCosineIterations",
                "The configured SCA iteration limit was reached."),
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
            throw new InvalidOperationException("SCA requires finite objective values.");
    }
}
