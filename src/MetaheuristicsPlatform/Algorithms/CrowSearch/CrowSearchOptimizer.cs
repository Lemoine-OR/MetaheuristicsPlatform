using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.CrowSearch;

public sealed class CrowSearchOptimizer :
    IMetaheuristic<double[], CrowSearchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.CrowSearch,
            Name = "Crow Search Algorithm",
            Acronym = "CSA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm | MetaheuristicMechanism.MemoryBased,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [CrowSearchReferences.Askarzadeh2016]
        };

    public CrowSearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        CrowSearchParameters parameters,
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
            throw new NotSupportedException("CSA requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        if (dimension <= 0)
            throw new InvalidOperationException("CSA requires a positive dimension.");

        int n = parameters.PopulationSize;
        double[][] positions = CreatePopulation(n, dimension);
        double[][] nextPositions = CreatePopulation(n, dimension);
        double[][] memories = CreatePopulation(n, dimension);
        double[] objectives = new double[n];
        double[] nextObjectives = new double[n];
        double[] memoryObjectives = new double[n];

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new CrowSearchState(
            0, CrowSearchPhase.Initialization, n, 0, null);
        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, positions[i]);
            objectives[i] = context.Evaluate(positions[i], state);
            RequireFinite(objectives[i]);
            Array.Copy(positions[i], memories[i], dimension);
            memoryObjectives[i] = objectives[i];
            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        int randomRelocations = 0;
        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            state = new CrowSearchState(
                iteration - 1,
                CrowSearchPhase.Search,
                n,
                randomRelocations,
                BestObjective(memoryObjectives, problem.Sense));

            for (int i = 0; i < n; i++)
            {
                int followedCrow;
                do { followedCrow = context.Random.NextInt32(n); } while (followedCrow == i);

                double awarenessDraw = context.Random.NextDouble();
                if (awarenessDraw >= parameters.AwarenessProbability)
                {
                    double r = context.Random.NextDouble();
                    for (int d = 0; d < dimension; d++)
                    {
                        nextPositions[i][d] =
                            positions[i][d] +
                            r * parameters.FlightLength *
                            (memories[followedCrow][d] - positions[i][d]);
                    }
                }
                else
                {
                    searchSpace.Sample(context.Random, nextPositions[i]);
                    randomRelocations++;
                }

                searchSpace.Clamp(nextPositions[i]);
                nextObjectives[i] = context.Evaluate(nextPositions[i], state);
                RequireFinite(nextObjectives[i]);

                if (problem.Sense.IsBetter(nextObjectives[i], memoryObjectives[i]))
                {
                    Array.Copy(nextPositions[i], memories[i], dimension);
                    memoryObjectives[i] = nextObjectives[i];
                }

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            (positions, nextPositions) = (nextPositions, positions);
            (objectives, nextObjectives) = (nextObjectives, objectives);

            double bestMemory = BestObjective(memoryObjectives, problem.Sense);
            state = new CrowSearchState(
                iteration,
                CrowSearchPhase.CompletedIteration,
                n,
                randomRelocations,
                bestMemory);

            context.CompleteIteration(bestMemory, state);
            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumCrowSearchIterations",
                "The configured CSA iteration limit was reached."),
            state);
    }

    private static double[][] CreatePopulation(int count, int dimension)
    {
        double[][] result = new double[count][];
        for (int i = 0; i < count; i++) result[i] = new double[dimension];
        return result;
    }

    private static double BestObjective(ReadOnlySpan<double> values, OptimizationSense sense)
    {
        double best = values[0];
        for (int i = 1; i < values.Length; i++)
            if (sense.IsBetter(values[i], best)) best = values[i];
        return best;
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
            throw new InvalidOperationException("CSA requires finite objective values.");
    }
}
