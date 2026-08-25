using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.SalpSwarm;

public sealed class SalpSwarmAlgorithmOptimizer :
    IMetaheuristic<double[], SalpSwarmAlgorithmParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.SalpSwarmAlgorithm,
            Name = "Salp Swarm Algorithm",
            Acronym = "SSA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [SalpSwarmAlgorithmReferences.MirjaliliGandomiMirjaliliSaremiFarisMirjalili2017]
        };

    public SalpSwarmAlgorithmParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        SalpSwarmAlgorithmParameters parameters,
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
            throw new NotSupportedException("SSA requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        ReadOnlySpan<double> lowerBounds = searchSpace.LowerBounds;
        ReadOnlySpan<double> upperBounds = searchSpace.UpperBounds;
        int dimension = searchSpace.Dimension;
        int n = parameters.PopulationSize;

        if (dimension <= 0)
            throw new InvalidOperationException("SSA requires a positive dimension.");

        double[][] salps = CreatePopulation(n, dimension);
        double[] objectives = new double[n];

        var context = new OptimizationContext<double[]>(
            Descriptor,
            problem,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);

        var state = new SalpSwarmAlgorithmState(
            0,
            SalpSwarmAlgorithmPhase.Initialization,
            n,
            2.0,
            null);

        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, salps[i]);
            objectives[i] = context.Evaluate(salps[i], state);
            RequireFinite(objectives[i]);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        int foodIndex = BestIndex(objectives, problem.Sense);
        double[] food = (double[])salps[foodIndex].Clone();
        double foodFitness = objectives[foodIndex];

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double sourceIteration = iteration + 1.0;
            double sourceMaximumIterations =
                parameters.MaximumIterations + 1.0;

            double c1 =
                2.0 *
                Math.Exp(
                    -Math.Pow(
                        4.0 * sourceIteration /
                        sourceMaximumIterations,
                        2.0));

            state = new SalpSwarmAlgorithmState(
                iteration - 1,
                SalpSwarmAlgorithmPhase.Search,
                n,
                c1,
                foodFitness);

            int leaderCount = n / 2;

            // The authors' MATLAB code updates the complete chain first.
            // Food is therefore frozen throughout this position-update loop.
            for (int i = 0; i < n; i++)
            {
                if (i < leaderCount)
                {
                    for (int d = 0; d < dimension; d++)
                    {
                        double c2 = context.Random.NextDouble();
                        double c3 = context.Random.NextDouble();
                        double displacement =
                            c1 *
                            (
                                (upperBounds[d] - lowerBounds[d]) * c2 +
                                lowerBounds[d]
                            );

                        salps[i][d] =
                            c3 < 0.5
                                ? food[d] + displacement
                                : food[d] - displacement;
                    }
                }
                else
                {
                    for (int d = 0; d < dimension; d++)
                    {
                        salps[i][d] =
                            0.5 *
                            (
                                salps[i][d] +
                                salps[i - 1][d]
                            );
                    }
                }
            }

            // Boundary repair, fitness evaluation and Food update are a
            // distinct second pass in the canonical source.
            for (int i = 0; i < n; i++)
            {
                searchSpace.Clamp(salps[i]);
                objectives[i] = context.Evaluate(salps[i], state);
                RequireFinite(objectives[i]);

                if (problem.Sense.IsBetter(objectives[i], foodFitness))
                {
                    foodFitness = objectives[i];
                    Array.Copy(salps[i], food, dimension);
                }

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            state = new SalpSwarmAlgorithmState(
                iteration,
                SalpSwarmAlgorithmPhase.CompletedIteration,
                n,
                c1,
                foodFitness);

            context.CompleteIteration(state.FoodFitness, state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumSalpSwarmIterations",
                "The configured SSA iteration limit was reached."),
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
            throw new InvalidOperationException("SSA requires finite objective values.");
    }
}
