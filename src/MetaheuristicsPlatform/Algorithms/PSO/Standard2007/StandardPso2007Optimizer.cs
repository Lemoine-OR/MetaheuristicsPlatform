using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.PSO.Standard2007;

/// <summary>
/// Standard PSO 2007 with the Bratton-Kennedy parameterization and
/// adaptive random informer graph.
/// </summary>
public sealed class StandardPso2007Optimizer :
    IMetaheuristic<double[], StandardPso2007Parameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.StandardParticleSwarm2007,
            Name = "Standard Particle Swarm Optimization 2007",
            Acronym = "SPSO-2007",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [StandardPso2007References.BrattonKennedy2007]
        };

    public StandardPso2007Parameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        StandardPso2007Parameters parameters,
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
            throw new NotSupportedException("SPSO-2007 requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        int n = parameters.ResolveSwarmSize(dimension);
        if (n <= 1) throw new InvalidOperationException("SPSO-2007 requires at least two particles.");

        double[] lower = searchSpace.LowerBounds.ToArray();
        double[] upper = searchSpace.UpperBounds.ToArray();

        double[][] positions = CreateMatrix(n, dimension);
        double[][] velocities = CreateMatrix(n, dimension);
        double[][] personalBest = CreateMatrix(n, dimension);
        double[] personalBestFitness = new double[n];
        bool[][] informs = CreateInformerGraph(n);

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new StandardPso2007State(
            0, n, parameters.ExpectedInformerCount, null);
        context.Start(state);

        for (int particle = 0; particle < n; particle++)
        {
            searchSpace.Sample(context.Random, positions[particle]);

            for (int d = 0; d < dimension; d++)
            {
                velocities[particle][d] =
                    (lower[d] - positions[particle][d]) +
                    context.Random.NextDouble() *
                    (upper[d] - lower[d]);
            }

            double fitness = context.Evaluate(positions[particle], state);
            RequireFinite(fitness);
            personalBestFitness[particle] = fitness;
            Array.Copy(positions[particle], personalBest[particle], dimension);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop) return context.Complete(stop, state);
        }

        RandomizeInformerGraph(informs, parameters.ExpectedInformerCount, context);
        double previousGlobalBest = personalBestFitness[BestIndex(personalBestFitness, problem.Sense)];

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int globalBestIndex = BestIndex(personalBestFitness, problem.Sense);
            double globalBestFitness = personalBestFitness[globalBestIndex];

            state = new StandardPso2007State(
                iteration - 1, n, parameters.ExpectedInformerCount, globalBestFitness);

            for (int particle = 0; particle < n; particle++)
            {
                int neighborhoodBest =
                    BestInformerForTarget(
                        particle,
                        informs,
                        personalBestFitness,
                        problem.Sense);

                for (int d = 0; d < dimension; d++)
                {
                    velocities[particle][d] =
                        parameters.InertiaWeight * velocities[particle][d] +
                        parameters.AccelerationCoefficient *
                            context.Random.NextDouble() *
                            (personalBest[particle][d] - positions[particle][d]) +
                        parameters.AccelerationCoefficient *
                            context.Random.NextDouble() *
                            (personalBest[neighborhoodBest][d] - positions[particle][d]);

                    positions[particle][d] += velocities[particle][d];
                }

                searchSpace.Clamp(positions[particle]);

                double fitness = context.Evaluate(positions[particle], state);
                RequireFinite(fitness);

                if (problem.Sense.IsBetter(fitness, personalBestFitness[particle]))
                {
                    personalBestFitness[particle] = fitness;
                    Array.Copy(positions[particle], personalBest[particle], dimension);
                }

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop) return context.Complete(stop, state);
            }

            globalBestIndex = BestIndex(personalBestFitness, problem.Sense);
            globalBestFitness = personalBestFitness[globalBestIndex];

            if (!problem.Sense.IsBetter(globalBestFitness, previousGlobalBest))
                RandomizeInformerGraph(informs, parameters.ExpectedInformerCount, context);

            previousGlobalBest = globalBestFitness;

            state = new StandardPso2007State(
                iteration, n, parameters.ExpectedInformerCount, globalBestFitness);
            context.CompleteIteration(globalBestFitness, state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop) return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumSpso2007Iterations",
                "The configured SPSO-2007 iteration limit was reached."),
            state);
    }

    private static void RandomizeInformerGraph(
        bool[][] informs,
        int informingAttempts,
        OptimizationContext<double[]> context)
    {
        int n = informs.Length;

        for (int source = 0; source < n; source++)
            Array.Clear(informs[source]);

        for (int source = 0; source < n; source++)
        {
            informs[source][source] = true;

            for (int attempt = 0; attempt < informingAttempts; attempt++)
            {
                int target = NextIndex(context, n);
                informs[source][target] = true;
            }
        }
    }

    private static int NextIndex(
        OptimizationContext<double[]> context,
        int count)
    {
        int index =
            (int)(context.Random.NextDouble() * count);

        return index >= count
            ? count - 1
            : index;
    }

    private static int BestInformerForTarget(
        int target,
        bool[][] informs,
        ReadOnlySpan<double> personalBestFitness,
        OptimizationSense sense)
    {
        int best = target;
        bool found = false;

        for (int source = 0; source < informs.Length; source++)
        {
            if (!informs[source][target])
                continue;

            if (!found ||
                sense.IsBetter(
                    personalBestFitness[source],
                    personalBestFitness[best]))
            {
                best = source;
                found = true;
            }
        }

        return best;
    }

    private static bool[][] CreateInformerGraph(int swarmSize)
    {
        bool[][] result = new bool[swarmSize][];

        for (int row = 0; row < swarmSize; row++)
            result[row] = new bool[swarmSize];

        return result;
    }

    private static double[][] CreateMatrix(int rows, int columns)
    {
        double[][] result = new double[rows][];
        for (int row = 0; row < rows; row++)
            result[row] = new double[columns];
        return result;
    }

    private static int BestIndex(ReadOnlySpan<double> values, OptimizationSense sense)
    {
        int best = 0;
        for (int i = 1; i < values.Length; i++)
            if (sense.IsBetter(values[i], values[best]))
                best = i;
        return best;
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
            throw new InvalidOperationException("SPSO-2007 requires finite objective values.");
    }
}
