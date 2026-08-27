using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.PSO.ComprehensiveLearning;

public sealed class ComprehensiveLearningParticleSwarmOptimizer :
    IMetaheuristic<double[], ComprehensiveLearningPsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ComprehensiveLearningParticleSwarm,
            Name = "Comprehensive Learning Particle Swarm Optimizer",
            Acronym = "CLPSO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [ComprehensiveLearningPsoReferences.LiangQinSuganthanBaskar2006]
        };

    public ComprehensiveLearningPsoParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        ComprehensiveLearningPsoParameters parameters,
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
            throw new NotSupportedException("CLPSO requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        int n = parameters.SwarmSize;
        double[] lower = searchSpace.LowerBounds.ToArray();
        double[] upper = searchSpace.UpperBounds.ToArray();

        double[][] positions = CreateMatrix(n, dimension);
        double[][] velocities = CreateMatrix(n, dimension);
        double[][] personalBest = CreateMatrix(n, dimension);
        double[] personalBestFitness = new double[n];
        int[][] exemplar = CreateIndexMatrix(n, dimension);
        int[] stagnation = new int[n];

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new ComprehensiveLearningPsoState(0, n, null);
        context.Start(state);

        for (int particle = 0; particle < n; particle++)
        {
            searchSpace.Sample(context.Random, positions[particle]);

            for (int d = 0; d < dimension; d++)
            {
                double maxVelocity =
                    (upper[d] - lower[d]) *
                    parameters.InitialVelocityRangeFraction;

                velocities[particle][d] =
                    (2.0 * context.Random.NextDouble() - 1.0) *
                    maxVelocity;
            }

            double fitness = context.Evaluate(positions[particle], state);
            RequireFinite(fitness);
            personalBestFitness[particle] = fitness;
            Array.Copy(positions[particle], personalBest[particle], dimension);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop) return context.Complete(stop, state);
        }

        for (int particle = 0; particle < n; particle++)
            RefreshExemplars(
                particle, exemplar[particle], personalBestFitness,
                problem.Sense, context, n, dimension);

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double inertia =
                ComprehensiveLearningPsoSchedule.InertiaWeight(
                    iteration - 1,
                    parameters.MaximumIterations,
                    parameters.InitialInertiaWeight,
                    parameters.FinalInertiaWeight);

            int bestIndex = BestIndex(personalBestFitness, problem.Sense);
            state = new ComprehensiveLearningPsoState(
                iteration - 1, n, personalBestFitness[bestIndex]);

            for (int particle = 0; particle < n; particle++)
            {
                if (stagnation[particle] >= parameters.RefreshingGap)
                {
                    RefreshExemplars(
                        particle, exemplar[particle], personalBestFitness,
                        problem.Sense, context, n, dimension);
                    stagnation[particle] = 0;
                }

                for (int d = 0; d < dimension; d++)
                {
                    int guide = exemplar[particle][d];
                    velocities[particle][d] =
                        inertia * velocities[particle][d] +
                        parameters.AccelerationCoefficient *
                        context.Random.NextDouble() *
                        (personalBest[guide][d] - positions[particle][d]);

                    positions[particle][d] += velocities[particle][d];
                }

                searchSpace.Clamp(positions[particle]);
                double fitness = context.Evaluate(positions[particle], state);
                RequireFinite(fitness);

                if (problem.Sense.IsBetter(fitness, personalBestFitness[particle]))
                {
                    personalBestFitness[particle] = fitness;
                    Array.Copy(positions[particle], personalBest[particle], dimension);
                    stagnation[particle] = 0;
                }
                else
                {
                    stagnation[particle]++;
                }

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop) return context.Complete(stop, state);
            }

            bestIndex = BestIndex(personalBestFitness, problem.Sense);
            state = new ComprehensiveLearningPsoState(iteration, n, personalBestFitness[bestIndex]);
            context.CompleteIteration(personalBestFitness[bestIndex], state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop) return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop("MaximumClpsoIterations", "The configured CLPSO iteration limit was reached."),
            state);
    }

    private static void RefreshExemplars(
        int particle,
        int[] destination,
        ReadOnlySpan<double> personalBestFitness,
        OptimizationSense sense,
        OptimizationContext<double[]> context,
        int swarmSize,
        int dimension)
    {
        double pc =
            ComprehensiveLearningPsoSchedule.LearningProbability(
                particle, swarmSize);

        bool learnsFromOther = false;

        for (int d = 0; d < dimension; d++)
        {
            if (context.Random.NextDouble() < pc)
            {
                int first = RandomOtherParticle(particle, swarmSize, context);
                int second = RandomOtherParticle(particle, swarmSize, context);
                while (second == first)
                    second = RandomOtherParticle(particle, swarmSize, context);

                destination[d] =
                    sense.IsBetter(
                        personalBestFitness[first],
                        personalBestFitness[second])
                        ? first
                        : second;

                learnsFromOther = true;
            }
            else
            {
                destination[d] = particle;
            }
        }

        if (!learnsFromOther)
        {
            int d = (int)(context.Random.NextDouble() * dimension);
            if (d >= dimension) d = dimension - 1;

            int first = RandomOtherParticle(particle, swarmSize, context);
            int second = RandomOtherParticle(particle, swarmSize, context);
            while (second == first)
                second = RandomOtherParticle(particle, swarmSize, context);

            destination[d] =
                sense.IsBetter(
                    personalBestFitness[first],
                    personalBestFitness[second])
                    ? first
                    : second;
        }
    }

    private static int RandomOtherParticle(
        int particle,
        int swarmSize,
        OptimizationContext<double[]> context)
    {
        int candidate;
        do
        {
            candidate = (int)(context.Random.NextDouble() * swarmSize);
            if (candidate >= swarmSize) candidate = swarmSize - 1;
        }
        while (candidate == particle);

        return candidate;
    }

    private static double[][] CreateMatrix(int rows, int columns)
    {
        double[][] result = new double[rows][];
        for (int row = 0; row < rows; row++) result[row] = new double[columns];
        return result;
    }

    private static int[][] CreateIndexMatrix(int rows, int columns)
    {
        int[][] result = new int[rows][];
        for (int row = 0; row < rows; row++) result[row] = new int[columns];
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
            throw new InvalidOperationException("CLPSO requires finite objective values.");
    }
}
