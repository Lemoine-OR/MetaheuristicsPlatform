using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.PSO.BareBones;

public sealed class BareBonesParticleSwarmOptimizer :
    IMetaheuristic<double[], BareBonesPsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.BareBonesParticleSwarm,
            Name = "Bare Bones Particle Swarm",
            Acronym = "BBPSO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [BareBonesPsoReferences.Kennedy2003]
        };

    public BareBonesPsoParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        BareBonesPsoParameters parameters,
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
            throw new NotSupportedException("Bare Bones PSO requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        int n = parameters.SwarmSize;

        double[][] positions = CreateMatrix(n, dimension);
        double[][] personalBest = CreateMatrix(n, dimension);
        double[] personalBestFitness = new double[n];
        double[] globalBest = new double[dimension];
        double[] candidate = new double[dimension];

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new BareBonesPsoState(0, n, null);
        context.Start(state);

        for (int particle = 0; particle < n; particle++)
        {
            searchSpace.Sample(context.Random, positions[particle]);
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

            int globalBestIndex = BestIndex(personalBestFitness, problem.Sense);
            Array.Copy(personalBest[globalBestIndex], globalBest, dimension);

            state = new BareBonesPsoState(
                iteration - 1, n, personalBestFitness[globalBestIndex]);

            for (int particle = 0; particle < n; particle++)
            {
                for (int d = 0; d < dimension; d++)
                {
                    BareBonesPsoDistribution distribution =
                        BareBonesPsoDistribution.From(
                            personalBest[particle][d],
                            globalBest[d]);

                    candidate[d] =
                        distribution.Mean +
                        distribution.StandardDeviation *
                        NextStandardNormal(context.Random.NextDouble(), context.Random.NextDouble());
                }

                searchSpace.Clamp(candidate);
                Array.Copy(candidate, positions[particle], dimension);

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
            state = new BareBonesPsoState(
                iteration, n, personalBestFitness[globalBestIndex]);
            context.CompleteIteration(personalBestFitness[globalBestIndex], state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop) return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop("MaximumBareBonesIterations", "The configured Bare Bones PSO iteration limit was reached."),
            state);
    }

    private static double NextStandardNormal(double u1, double u2)
    {
        double safeU1 = Math.Max(u1, double.Epsilon);
        return Math.Sqrt(-2.0 * Math.Log(safeU1)) * Math.Cos(2.0 * Math.PI * u2);
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
            throw new InvalidOperationException("Bare Bones PSO requires finite objective values.");
    }
}
