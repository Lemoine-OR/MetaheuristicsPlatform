using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

public sealed class FullyInformedParticleSwarmOptimizer :
    IMetaheuristic<double[], FullyInformedPsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.FullyInformedParticleSwarm,
            Name = "Fully Informed Particle Swarm",
            Acronym = "FIPS",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [FullyInformedPsoReferences.MendesKennedyNeves2004]
        };

    public FullyInformedPsoParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        FullyInformedPsoParameters parameters,
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
            throw new NotSupportedException("FIPS requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        int n = parameters.SwarmSize;
        double[] lower = searchSpace.LowerBounds.ToArray();
        double[] upper = searchSpace.UpperBounds.ToArray();

        double[][] positions = CreateMatrix(n, dimension);
        double[][] velocities = CreateMatrix(n, dimension);
        double[][] personalBest = CreateMatrix(n, dimension);
        double[] personalBestFitness = new double[n];

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new FullyInformedPsoState(0, n, null);
        context.Start(state);

        for (int particle = 0; particle < n; particle++)
        {
            searchSpace.Sample(context.Random, positions[particle]);
            for (int d = 0; d < dimension; d++)
            {
                double maxVelocity =
                    (upper[d] - lower[d]) * parameters.InitialVelocityRangeFraction;
                velocities[particle][d] =
                    (2.0 * context.Random.NextDouble() - 1.0) * maxVelocity;
            }

            double fitness = context.Evaluate(positions[particle], state);
            RequireFinite(fitness);
            personalBestFitness[particle] = fitness;
            Array.Copy(positions[particle], personalBest[particle], dimension);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop) return context.Complete(stop, state);
        }

        var dynamics = new ClercKennedyConstrictionDynamics(
            parameters.TotalAccelerationCoefficient,
            parameters.Kappa);

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PsoVelocityCoefficients coefficients =
                dynamics.GetCoefficients(iteration - 1L);

            double coefficientPerInformer =
                FullyInformedPsoKernel.CoefficientPerInformer(
                    parameters.TotalAccelerationCoefficient,
                    n);

            int bestIndex = BestIndex(personalBestFitness, problem.Sense);
            state = new FullyInformedPsoState(
                iteration - 1, n, personalBestFitness[bestIndex]);

            for (int particle = 0; particle < n; particle++)
            {
                for (int d = 0; d < dimension; d++)
                {
                    double attraction = 0.0;
                    double current = positions[particle][d];

                    for (int informer = 0; informer < n; informer++)
                    {
                        attraction +=
                            coefficientPerInformer *
                            context.Random.NextDouble() *
                            (personalBest[informer][d] - current);
                    }

                    velocities[particle][d] =
                        coefficients.PreviousVelocityMultiplier *
                            velocities[particle][d] +
                        coefficients.AttractionMultiplier *
                            attraction;

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

            bestIndex = BestIndex(personalBestFitness, problem.Sense);
            state = new FullyInformedPsoState(iteration, n, personalBestFitness[bestIndex]);
            context.CompleteIteration(personalBestFitness[bestIndex], state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop) return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop("MaximumFipsIterations", "The configured FIPS iteration limit was reached."),
            state);
    }

    private static double[][] CreateMatrix(int rows, int columns)
    {
        double[][] result = new double[rows][];
        for (int row = 0; row < rows; row++) result[row] = new double[columns];
        return result;
    }

    private static int BestIndex(ReadOnlySpan<double> values, OptimizationSense sense)
    {
        int best = 0;
        for (int i = 1; i < values.Length; i++)
            if (sense.IsBetter(values[i], values[best])) best = i;
        return best;
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
            throw new InvalidOperationException("FIPS requires finite objective values.");
    }
}
