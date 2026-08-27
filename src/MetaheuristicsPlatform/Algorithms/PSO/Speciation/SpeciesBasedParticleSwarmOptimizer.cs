using System.Linq;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.PSO.Speciation;

/// <summary>
/// Species-based PSO in the static multimodal mode of Parrott and Li (2006).
/// Species seeds are reconstructed each iteration from dominating personal bests.
/// </summary>
public sealed class SpeciesBasedParticleSwarmOptimizer :
    IMetaheuristic<double[], SpeciesBasedPsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.SpeciesBasedParticleSwarm,
            Name = "Species-Based Particle Swarm Optimization",
            Acronym = "SPSO-Species",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [SpeciesBasedPsoReferences.ParrottLi2006]
        };

    public SpeciesBasedPsoParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        SpeciesBasedPsoParameters parameters,
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
            throw new NotSupportedException("Species-based PSO requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        int n = parameters.SwarmSize;
        double[] lower = searchSpace.LowerBounds.ToArray();
        double[] upper = searchSpace.UpperBounds.ToArray();

        double[][] positions = CreateMatrix(n, dimension);
        double[][] velocities = CreateMatrix(n, dimension);
        double[][] personalBest = CreateMatrix(n, dimension);
        double[] personalBestFitness = new double[n];

        double diagonal = 0.0;
        for (int d = 0; d < dimension; d++)
        {
            double width = upper[d] - lower[d];
            diagonal += width * width;
        }
        double speciesRadius = Math.Sqrt(diagonal) * parameters.SpeciesRadiusFraction;

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new SpeciesBasedPsoState(0, n, 0, null);
        context.Start(state);

        for (int particle = 0; particle < n; particle++)
        {
            searchSpace.Sample(context.Random, positions[particle]);

            for (int d = 0; d < dimension; d++)
            {
                double vmax =
                    (upper[d] - lower[d]) *
                    parameters.InitialVelocityRangeFraction;

                velocities[particle][d] =
                    (2.0 * context.Random.NextDouble() - 1.0) * vmax;
            }

            double fitness = context.Evaluate(positions[particle], state);
            RequireFinite(fitness);
            personalBestFitness[particle] = fitness;
            Array.Copy(positions[particle], personalBest[particle], dimension);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop) return context.Complete(stop, state);
        }

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int[] seedForParticle =
                SpeciesPartitioner.AssignSpeciesSeeds(
                    personalBest,
                    personalBestFitness,
                    problem.Sense,
                    speciesRadius);

            int speciesCount = seedForParticle.Distinct().Count();
            int globalBest = BestIndex(personalBestFitness, problem.Sense);

            state = new SpeciesBasedPsoState(
                iteration - 1, n, speciesCount, personalBestFitness[globalBest]);

            for (int particle = 0; particle < n; particle++)
            {
                int seed = seedForParticle[particle];

                for (int d = 0; d < dimension; d++)
                {
                    velocities[particle][d] =
                        parameters.InertiaWeight * velocities[particle][d] +
                        parameters.CognitiveCoefficient *
                            context.Random.NextDouble() *
                            (personalBest[particle][d] - positions[particle][d]) +
                        parameters.SocialCoefficient *
                            context.Random.NextDouble() *
                            (personalBest[seed][d] - positions[particle][d]);

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

            globalBest = BestIndex(personalBestFitness, problem.Sense);
            state = new SpeciesBasedPsoState(
                iteration, n, speciesCount, personalBestFitness[globalBest]);
            context.CompleteIteration(personalBestFitness[globalBest], state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop) return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumSpeciesPsoIterations",
                "The configured species-based PSO iteration limit was reached."),
            state);
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
            throw new InvalidOperationException("Species-based PSO requires finite objective values.");
    }
}
