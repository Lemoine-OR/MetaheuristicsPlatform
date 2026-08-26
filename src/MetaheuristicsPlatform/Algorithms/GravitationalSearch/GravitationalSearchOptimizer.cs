using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.GravitationalSearch;

public sealed class GravitationalSearchOptimizer :
    IMetaheuristic<double[], GravitationalSearchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.GravitationalSearch,
            Name = "Gravitational Search Algorithm",
            Acronym = "GSA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [GravitationalSearchReferences.RashediNezamabadiPourSaryazdi2009]
        };

    public GravitationalSearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        GravitationalSearchParameters parameters,
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
            throw new NotSupportedException("GSA requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        if (dimension <= 0)
            throw new InvalidOperationException("GSA requires a positive dimension.");

        int n = parameters.PopulationSize;
        double[][] positions = CreatePopulation(n, dimension);
        double[][] nextPositions = CreatePopulation(n, dimension);
        double[][] velocities = CreatePopulation(n, dimension);
        double[][] nextVelocities = CreatePopulation(n, dimension);
        double[] objectives = new double[n];
        double[] nextObjectives = new double[n];
        double[] masses = new double[n];
        double[][] accelerations = CreatePopulation(n, dimension);

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new GravitationalSearchState(
            0, GravitationalSearchPhase.Initialization, n, n,
            parameters.InitialGravitationalConstant, null);
        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, positions[i]);
            objectives[i] = context.Evaluate(positions[i], state);
            RequireFinite(objectives[i]);
            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double gravity =
                parameters.InitialGravitationalConstant *
                Math.Exp(-parameters.GravityDecay * iteration / parameters.MaximumIterations);

            int kBest = Math.Clamp(
                (int)Math.Round(
                    n * (2.0 + (1.0 - (double)iteration / parameters.MaximumIterations) * 98.0) / 100.0,
                    MidpointRounding.AwayFromZero),
                1,
                n);

            ComputeNormalizedMasses(objectives, problem.Sense, masses);
            int[] massOrder = RankMassesDescending(masses);

            for (int i = 0; i < n; i++)
                Array.Clear(accelerations[i], 0, dimension);

            for (int i = 0; i < n; i++)
            {
                for (int rank = 0; rank < kBest; rank++)
                {
                    int j = massOrder[rank];
                    if (j == i) continue;

                    double distanceSquared = 0.0;
                    for (int d = 0; d < dimension; d++)
                    {
                        double delta = positions[j][d] - positions[i][d];
                        distanceSquared += delta * delta;
                    }

                    double distance = Math.Sqrt(distanceSquared);
                    double randomWeight = context.Random.NextDouble();
                    double coefficient =
                        randomWeight * gravity * masses[j] /
                        (distance + parameters.DistanceEpsilon);

                    for (int d = 0; d < dimension; d++)
                        accelerations[i][d] += coefficient * (positions[j][d] - positions[i][d]);
                }
            }

            state = new GravitationalSearchState(
                iteration - 1,
                GravitationalSearchPhase.Search,
                n,
                kBest,
                gravity,
                BestObjective(objectives, problem.Sense));

            for (int i = 0; i < n; i++)
            {
                double inertiaRandom = context.Random.NextDouble();
                for (int d = 0; d < dimension; d++)
                {
                    nextVelocities[i][d] =
                        inertiaRandom * velocities[i][d] + accelerations[i][d];
                    nextPositions[i][d] =
                        positions[i][d] + nextVelocities[i][d];
                }

                searchSpace.Clamp(nextPositions[i]);
                nextObjectives[i] = context.Evaluate(nextPositions[i], state);
                RequireFinite(nextObjectives[i]);

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            (positions, nextPositions) = (nextPositions, positions);
            (velocities, nextVelocities) = (nextVelocities, velocities);
            (objectives, nextObjectives) = (nextObjectives, objectives);

            state = new GravitationalSearchState(
                iteration,
                GravitationalSearchPhase.CompletedIteration,
                n,
                kBest,
                gravity,
                BestObjective(objectives, problem.Sense));

            context.CompleteIteration(state.BestFitness, state);
            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumGravitationalSearchIterations",
                "The configured GSA iteration limit was reached."),
            state);
    }

    private static double[][] CreatePopulation(int count, int dimension)
    {
        double[][] result = new double[count][];
        for (int i = 0; i < count; i++) result[i] = new double[dimension];
        return result;
    }

    private static void ComputeNormalizedMasses(
        ReadOnlySpan<double> objectives,
        OptimizationSense sense,
        Span<double> masses)
    {
        double best = objectives[0];
        double worst = objectives[0];
        for (int i = 1; i < objectives.Length; i++)
        {
            if (sense.IsBetter(objectives[i], best)) best = objectives[i];
            if (sense.IsBetter(worst, objectives[i])) worst = objectives[i];
        }

        double denominator = best - worst;
        if (Math.Abs(denominator) <= 1e-15)
        {
            double uniform = 1.0 / objectives.Length;
            for (int i = 0; i < masses.Length; i++) masses[i] = uniform;
            return;
        }

        double sum = 0.0;
        for (int i = 0; i < objectives.Length; i++)
        {
            masses[i] = (objectives[i] - worst) / denominator;
            if (masses[i] < 0.0 && masses[i] > -1e-12) masses[i] = 0.0;
            sum += masses[i];
        }

        if (!(sum > 0.0) || !double.IsFinite(sum))
            throw new InvalidOperationException("GSA mass normalization failed.");

        for (int i = 0; i < masses.Length; i++) masses[i] /= sum;
    }

    private static int[] RankMassesDescending(ReadOnlySpan<double> masses)
    {
        int[] order = Enumerable.Range(0, masses.Length).ToArray();
        double[] snapshot = masses.ToArray();
        Array.Sort(order, (left, right) =>
        {
            int cmp = snapshot[right].CompareTo(snapshot[left]);
            return cmp != 0 ? cmp : left.CompareTo(right);
        });
        return order;
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
            throw new InvalidOperationException("GSA requires finite objective values.");
    }
}
