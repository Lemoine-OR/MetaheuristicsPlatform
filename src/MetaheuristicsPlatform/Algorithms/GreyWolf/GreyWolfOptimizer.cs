using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.GreyWolf;

public sealed class GreyWolfOptimizer :
    IMetaheuristic<double[], GreyWolfOptimizerParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.GreyWolfOptimizer,
            Name = "Grey Wolf Optimizer",
            Acronym = "GWO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [GreyWolfOptimizerReferences.MirjaliliMirjaliliLewis2014]
        };

    public GreyWolfOptimizerParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        GreyWolfOptimizerParameters parameters,
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
            throw new NotSupportedException(
                "GWO requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
            throw new InvalidOperationException(
                "GWO requires a positive dimension.");

        int n =
            parameters.PopulationSize;

        double[][] wolves =
            new double[n][];

        double[][] next =
            new double[n][];

        double[] objectives =
            new double[n];

        for (int i = 0; i < n; i++)
        {
            wolves[i] =
                new double[dimension];

            next[i] =
                new double[dimension];
        }

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        var state =
            new GreyWolfOptimizerState(
                0,
                GreyWolfOptimizerPhase.Initialization,
                n,
                2.0,
                null,
                null,
                null);

        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(
                context.Random,
                wolves[i]);

            objectives[i] =
                context.Evaluate(
                    wolves[i],
                    state);

            RequireFinite(objectives[i]);

            StoppingDecision stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int[] leaders =
                BestThree(
                    objectives,
                    problem.Sense);

            double[] alpha =
                wolves[leaders[0]];

            double[] beta =
                wolves[leaders[1]];

            double[] delta =
                wolves[leaders[2]];

            double a =
                parameters.MaximumIterations == 1
                    ? 0.0
                    : 2.0 -
                      (
                          2.0 *
                          (iteration - 1.0) /
                          (parameters.MaximumIterations - 1.0)
                      );

            state =
                new GreyWolfOptimizerState(
                    iteration - 1,
                    GreyWolfOptimizerPhase.Search,
                    n,
                    a,
                    objectives[leaders[0]],
                    objectives[leaders[1]],
                    objectives[leaders[2]]);

            for (int i = 0; i < n; i++)
            {
                for (int d = 0; d < dimension; d++)
                {
                    double x1 =
                        Encircle(
                            alpha[d],
                            wolves[i][d],
                            a,
                            context.Random.NextDouble(),
                            context.Random.NextDouble());

                    double x2 =
                        Encircle(
                            beta[d],
                            wolves[i][d],
                            a,
                            context.Random.NextDouble(),
                            context.Random.NextDouble());

                    double x3 =
                        Encircle(
                            delta[d],
                            wolves[i][d],
                            a,
                            context.Random.NextDouble(),
                            context.Random.NextDouble());

                    next[i][d] =
                        (x1 + x2 + x3) /
                        3.0;
                }

                searchSpace.Clamp(
                    next[i].AsSpan());

                double nextObjective =
                    context.Evaluate(
                        next[i],
                        state);

                RequireFinite(nextObjective);

                objectives[i] =
                    nextObjective;

                StoppingDecision stop =
                    context.EvaluateStopping(state);

                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            (wolves, next) =
                (next, wolves);

            leaders =
                BestThree(
                    objectives,
                    problem.Sense);

            state =
                new GreyWolfOptimizerState(
                    iteration,
                    GreyWolfOptimizerPhase.CompletedIteration,
                    n,
                    a,
                    objectives[leaders[0]],
                    objectives[leaders[1]],
                    objectives[leaders[2]]);

            context.CompleteIteration(
                state.AlphaFitness,
                state);

            StoppingDecision iterationStop =
                context.EvaluateStopping(state);

            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumGreyWolfIterations",
                "The configured GWO iteration limit was reached."),
            state);
    }

    private static double Encircle(
        double leader,
        double current,
        double a,
        double r1,
        double r2)
    {
        double coefficientA =
            (2.0 * a * r1) -
            a;

        double coefficientC =
            2.0 * r2;

        double distance =
            Math.Abs(
                (coefficientC * leader) -
                current);

        return
            leader -
            (coefficientA * distance);
    }

    private static int[] BestThree(
        ReadOnlySpan<double> values,
        OptimizationSense sense)
    {
        int[] order =
            Enumerable.Range(
                0,
                values.Length).ToArray();

        double[] snapshot =
            values.ToArray();

        Array.Sort(
            order,
            (left, right) =>
            {
                if (snapshot[left] == snapshot[right])
                    return left.CompareTo(right);

                return sense.IsBetter(
                    snapshot[left],
                    snapshot[right])
                    ? -1
                    : 1;
            });

        return
        [
            order[0],
            order[1],
            order[2]
        ];
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new InvalidOperationException(
                "GWO requires finite objective values.");
        }
    }
}
