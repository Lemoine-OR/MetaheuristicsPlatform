using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

internal static class ScientificCanonicalPsoRunner
{
    internal static OptimizationResult<double[]> Optimize(
        MetaheuristicDescriptor descriptor,
        string variant,
        IOptimizationProblem<double[]> problem,
        int swarmSize,
        int maximumIterations,
        double cognitiveCoefficient,
        double socialCoefficient,
        double initialVelocityRangeFraction,
        IPsoVelocityDynamics dynamics,
        ISolutionCloner<double[]> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options,
        IOptimizationCallback<double[]>? callback,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(dynamics);
        ArgumentNullException.ThrowIfNull(solutionCloner);
        ArgumentNullException.ThrowIfNull(stoppingCriterion);

        if (problem is not ISpanContinuousOptimizationProblem continuousProblem)
            throw new NotSupportedException("Scientific PSO variants require ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        double[] lower = searchSpace.LowerBounds.ToArray();
        double[] upper = searchSpace.UpperBounds.ToArray();

        double[][] positions = CreateMatrix(swarmSize, dimension);
        double[][] velocities = CreateMatrix(swarmSize, dimension);
        double[][] personalBest = CreateMatrix(swarmSize, dimension);
        double[] personalBestFitness = new double[swarmSize];

        var context = new OptimizationContext<double[]>(
            descriptor,
            problem,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);

        var state = new ScientificCanonicalPsoState(0, variant, swarmSize, null);
        context.Start(state);

        for (int particle = 0; particle < swarmSize; particle++)
        {
            searchSpace.Sample(context.Random, positions[particle]);

            for (int d = 0; d < dimension; d++)
            {
                double maxVelocity =
                    (upper[d] - lower[d]) *
                    initialVelocityRangeFraction;

                velocities[particle][d] =
                    (2.0 * context.Random.NextDouble() - 1.0) *
                    maxVelocity;
            }

            double fitness = context.Evaluate(positions[particle], state);
            RequireFinite(fitness);
            personalBestFitness[particle] = fitness;
            Array.Copy(positions[particle], personalBest[particle], dimension);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        double[] globalBest = new double[dimension];

        for (int iteration = 1; iteration <= maximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int bestIndex = BestIndex(personalBestFitness, problem.Sense);
            Array.Copy(personalBest[bestIndex], globalBest, dimension);

            PsoVelocityCoefficients coefficients =
                dynamics.GetCoefficients(iteration - 1L);

            state = new ScientificCanonicalPsoState(
                iteration - 1,
                variant,
                swarmSize,
                personalBestFitness[bestIndex]);

            for (int particle = 0; particle < swarmSize; particle++)
            {
                for (int d = 0; d < dimension; d++)
                {
                    double attraction =
                        cognitiveCoefficient *
                            context.Random.NextDouble() *
                            (personalBest[particle][d] - positions[particle][d]) +
                        socialCoefficient *
                            context.Random.NextDouble() *
                            (globalBest[d] - positions[particle][d]);

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
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            bestIndex = BestIndex(personalBestFitness, problem.Sense);
            state = new ScientificCanonicalPsoState(
                iteration,
                variant,
                swarmSize,
                personalBestFitness[bestIndex]);

            context.CompleteIteration(personalBestFitness[bestIndex], state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumScientificPsoIterations",
                "The configured scientific PSO iteration limit was reached."),
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
            throw new InvalidOperationException("Scientific PSO variants require finite objective values.");
    }
}
