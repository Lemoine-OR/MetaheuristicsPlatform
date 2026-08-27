using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.PSO.Cooperative;

/// <summary>
/// Cooperative PSO using the CPSO-SK component decomposition of
/// van den Bergh and Engelbrecht (2004).
/// </summary>
public sealed class CooperativeParticleSwarmOptimizer :
    IMetaheuristic<double[], CooperativePsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.CooperativeParticleSwarm,
            Name = "Cooperative Particle Swarm Optimization (CPSO-SK)",
            Acronym = "CPSO-SK",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [CooperativePsoReferences.VanDenBerghEngelbrecht2004]
        };

    public CooperativePsoParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        CooperativePsoParameters parameters,
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
            throw new NotSupportedException("CPSO-SK requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        int subSwarmCount = Math.Min(parameters.SubswarmCount, dimension);
        int subSwarmSize = parameters.SubswarmSize;

        int[] starts = new int[subSwarmCount];
        int[] lengths = new int[subSwarmCount];
        BuildBalancedPartition(dimension, subSwarmCount, starts, lengths);

        double[] lower = searchSpace.LowerBounds.ToArray();
        double[] upper = searchSpace.UpperBounds.ToArray();

        double[][][] positions = new double[subSwarmCount][][];
        double[][][] velocities = new double[subSwarmCount][][];
        double[][][] personalBest = new double[subSwarmCount][][];
        double[][] personalBestFitness = new double[subSwarmCount][];
        double[][] bestComponent = new double[subSwarmCount][];
        double[] contextVector = new double[dimension];
        double[] candidate = new double[dimension];

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new CooperativePsoState(0, subSwarmCount, subSwarmSize, null);
        context.Start(state);

        searchSpace.Sample(context.Random, contextVector);

        for (int s = 0; s < subSwarmCount; s++)
        {
            int length = lengths[s];
            positions[s] = CreateMatrix(subSwarmSize, length);
            velocities[s] = CreateMatrix(subSwarmSize, length);
            personalBest[s] = CreateMatrix(subSwarmSize, length);
            personalBestFitness[s] = new double[subSwarmSize];
            bestComponent[s] = new double[length];

            for (int p = 0; p < subSwarmSize; p++)
            {
                for (int d = 0; d < length; d++)
                {
                    int globalD = starts[s] + d;
                    positions[s][p][d] =
                        lower[globalD] +
                        context.Random.NextDouble() *
                        (upper[globalD] - lower[globalD]);

                    double vmax =
                        (upper[globalD] - lower[globalD]) *
                        parameters.InitialVelocityRangeFraction;

                    velocities[s][p][d] =
                        (2.0 * context.Random.NextDouble() - 1.0) * vmax;
                }

                Compose(candidate, contextVector, positions[s][p], starts[s], length);
                double fitness = context.Evaluate(candidate, state);
                RequireFinite(fitness);
                personalBestFitness[s][p] = fitness;
                Array.Copy(positions[s][p], personalBest[s][p], length);

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop) return context.Complete(stop, state);
            }

            int best = BestIndex(personalBestFitness[s], problem.Sense);
            Array.Copy(personalBest[s][best], bestComponent[s], length);
            CopyComponent(bestComponent[s], contextVector, starts[s], length);
        }

        double contextFitness = EvaluateContext(contextVector, context, state);

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            state = new CooperativePsoState(
                iteration - 1, subSwarmCount, subSwarmSize, contextFitness);

            for (int s = 0; s < subSwarmCount; s++)
            {
                int length = lengths[s];
                int best = BestIndex(personalBestFitness[s], problem.Sense);
                Array.Copy(personalBest[s][best], bestComponent[s], length);

                for (int p = 0; p < subSwarmSize; p++)
                {
                    for (int d = 0; d < length; d++)
                    {
                        velocities[s][p][d] =
                            parameters.InertiaWeight * velocities[s][p][d] +
                            parameters.CognitiveCoefficient *
                                context.Random.NextDouble() *
                                (personalBest[s][p][d] - positions[s][p][d]) +
                            parameters.SocialCoefficient *
                                context.Random.NextDouble() *
                                (bestComponent[s][d] - positions[s][p][d]);

                        positions[s][p][d] += velocities[s][p][d];

                        int globalD = starts[s] + d;
                        positions[s][p][d] =
                            Math.Clamp(positions[s][p][d], lower[globalD], upper[globalD]);
                    }

                    Compose(candidate, contextVector, positions[s][p], starts[s], length);
                    double fitness = context.Evaluate(candidate, state);
                    RequireFinite(fitness);

                    if (problem.Sense.IsBetter(fitness, personalBestFitness[s][p]))
                    {
                        personalBestFitness[s][p] = fitness;
                        Array.Copy(positions[s][p], personalBest[s][p], length);
                    }

                    StoppingDecision stop = context.EvaluateStopping(state);
                    if (stop.ShouldStop) return context.Complete(stop, state);
                }

                best = BestIndex(personalBestFitness[s], problem.Sense);
                Array.Copy(personalBest[s][best], bestComponent[s], length);
                CopyComponent(bestComponent[s], contextVector, starts[s], length);
                contextFitness = EvaluateContext(contextVector, context, state);

                StoppingDecision contextStop = context.EvaluateStopping(state);
                if (contextStop.ShouldStop) return context.Complete(contextStop, state);
            }

            state = new CooperativePsoState(
                iteration, subSwarmCount, subSwarmSize, contextFitness);
            context.CompleteIteration(contextFitness, state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop) return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumCpsoIterations",
                "The configured CPSO-SK iteration limit was reached."),
            state);
    }

    private static double EvaluateContext(
        double[] vector,
        OptimizationContext<double[]> context,
        CooperativePsoState state)
    {
        double value = context.Evaluate(vector, state);
        RequireFinite(value);
        return value;
    }

    private static void BuildBalancedPartition(
        int dimension,
        int groups,
        int[] starts,
        int[] lengths)
    {
        int baseLength = dimension / groups;
        int remainder = dimension % groups;
        int offset = 0;

        for (int group = 0; group < groups; group++)
        {
            int length = baseLength + (group < remainder ? 1 : 0);
            starts[group] = offset;
            lengths[group] = length;
            offset += length;
        }
    }

    private static void Compose(
        double[] destination,
        double[] context,
        double[] component,
        int start,
        int length)
    {
        Array.Copy(context, destination, context.Length);
        Array.Copy(component, 0, destination, start, length);
    }

    private static void CopyComponent(
        double[] component,
        double[] context,
        int start,
        int length) =>
        Array.Copy(component, 0, context, start, length);

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
            throw new InvalidOperationException("CPSO-SK requires finite objective values.");
    }
}
