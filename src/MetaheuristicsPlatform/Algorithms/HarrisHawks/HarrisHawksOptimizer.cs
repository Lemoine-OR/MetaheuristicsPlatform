using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.HarrisHawks;

public sealed class HarrisHawksOptimizer :
    IMetaheuristic<double[], HarrisHawksOptimizerParameters>
{
    private const double MantegnaSigmaBeta15 =
        0.6965745025576968;

    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.HarrisHawksOptimization,
            Name = "Harris Hawks Optimization",
            Acronym = "HHO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [HarrisHawksOptimizerReferences.HeidariMirjaliliFarisAljarahMafarjaChen2019]
        };

    public HarrisHawksOptimizerParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        HarrisHawksOptimizerParameters parameters,
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
            throw new NotSupportedException("HHO requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        ReadOnlySpan<double> lowerBounds = searchSpace.LowerBounds;
        ReadOnlySpan<double> upperBounds = searchSpace.UpperBounds;
        int dimension = searchSpace.Dimension;
        int n = parameters.PopulationSize;

        if (dimension <= 0)
            throw new InvalidOperationException("HHO requires a positive dimension.");

        double[][] hawks = CreatePopulation(n, dimension);
        double[] objectives = new double[n];

        var context = new OptimizationContext<double[]>(
            Descriptor,
            problem,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);

        var state = new HarrisHawksOptimizerState(
            0,
            HarrisHawksOptimizerPhase.Initialization,
            n,
            2.0,
            0,
            null);

        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, hawks[i]);
            objectives[i] = context.Evaluate(hawks[i], state);
            RequireFinite(objectives[i]);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        int rapidDiveEvaluations = 0;
        double[] candidate = new double[dimension];
        double[] secondCandidate = new double[dimension];
        double[] mean = new double[dimension];

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int rabbitIndex = BestIndex(objectives, problem.Sense);
            double[] rabbit = (double[])hawks[rabbitIndex].Clone();
            double rabbitFitness = objectives[rabbitIndex];

            double sourceTime = iteration - 1.0;
            double e1 =
                2.0 *
                (1.0 - sourceTime / parameters.MaximumIterations);

            state = new HarrisHawksOptimizerState(
                iteration - 1,
                HarrisHawksOptimizerPhase.Search,
                n,
                e1,
                rapidDiveEvaluations,
                rabbitFitness);

            for (int i = 0; i < n; i++)
            {
                double e0 = 2.0 * context.Random.NextDouble() - 1.0;
                double escapingEnergy = e1 * e0;
                double absoluteEnergy = Math.Abs(escapingEnergy);
                double candidateObjective;

                if (absoluteEnergy >= 1.0)
                {
                    double q = context.Random.NextDouble();
                    int randomIndex = context.Random.NextInt32(n);

                    if (q < 0.5)
                    {
                        double r1 = context.Random.NextDouble();
                        double r2 = context.Random.NextDouble();

                        for (int d = 0; d < dimension; d++)
                        {
                            candidate[d] =
                                hawks[randomIndex][d] -
                                r1 *
                                Math.Abs(
                                    hawks[randomIndex][d] -
                                    2.0 * r2 * hawks[i][d]);
                        }
                    }
                    else
                    {
                        MeanPosition(hawks, mean);
                        double r3 = context.Random.NextDouble();
                        double r4 = context.Random.NextDouble();

                        for (int d = 0; d < dimension; d++)
                        {
                            double homeRangeSample =
                                (upperBounds[d] - lowerBounds[d]) * r4 +
                                lowerBounds[d];

                            candidate[d] =
                                (rabbit[d] - mean[d]) -
                                r3 * homeRangeSample;
                        }
                    }

                    searchSpace.Clamp(candidate);
                    candidateObjective = context.Evaluate(candidate, state);
                    RequireFinite(candidateObjective);
                    Array.Copy(candidate, hawks[i], dimension);
                    objectives[i] = candidateObjective;
                }
                else
                {
                    double r = context.Random.NextDouble();

                    if (r >= 0.5 && absoluteEnergy < 0.5)
                    {
                        for (int d = 0; d < dimension; d++)
                        {
                            candidate[d] =
                                rabbit[d] -
                                escapingEnergy *
                                Math.Abs(rabbit[d] - hawks[i][d]);
                        }

                        searchSpace.Clamp(candidate);
                        candidateObjective = context.Evaluate(candidate, state);
                        RequireFinite(candidateObjective);
                        Array.Copy(candidate, hawks[i], dimension);
                        objectives[i] = candidateObjective;
                    }
                    else if (r >= 0.5)
                    {
                        double jumpStrength =
                            2.0 *
                            (1.0 - context.Random.NextDouble());

                        for (int d = 0; d < dimension; d++)
                        {
                            candidate[d] =
                                (rabbit[d] - hawks[i][d]) -
                                escapingEnergy *
                                Math.Abs(
                                    jumpStrength * rabbit[d] -
                                    hawks[i][d]);
                        }

                        searchSpace.Clamp(candidate);
                        candidateObjective = context.Evaluate(candidate, state);
                        RequireFinite(candidateObjective);
                        Array.Copy(candidate, hawks[i], dimension);
                        objectives[i] = candidateObjective;
                    }
                    else
                    {
                        double jumpStrength =
                            2.0 *
                            (1.0 - context.Random.NextDouble());

                        if (absoluteEnergy >= 0.5)
                        {
                            for (int d = 0; d < dimension; d++)
                            {
                                candidate[d] =
                                    rabbit[d] -
                                    escapingEnergy *
                                    Math.Abs(
                                        jumpStrength * rabbit[d] -
                                        hawks[i][d]);
                            }
                        }
                        else
                        {
                            MeanPosition(hawks, mean);

                            for (int d = 0; d < dimension; d++)
                            {
                                candidate[d] =
                                    rabbit[d] -
                                    escapingEnergy *
                                    Math.Abs(
                                        jumpStrength * rabbit[d] -
                                        mean[d]);
                            }
                        }

                        searchSpace.Clamp(candidate);
                        candidateObjective = context.Evaluate(candidate, state);
                        RequireFinite(candidateObjective);
                        rapidDiveEvaluations++;

                        StoppingDecision firstDiveStop = context.EvaluateStopping(
                            state with { RapidDiveEvaluations = rapidDiveEvaluations });

                        if (firstDiveStop.ShouldStop)
                            return context.Complete(firstDiveStop, state);

                        if (problem.Sense.IsBetter(candidateObjective, objectives[i]))
                        {
                            Array.Copy(candidate, hawks[i], dimension);
                            objectives[i] = candidateObjective;
                        }
                        else
                        {
                            for (int d = 0; d < dimension; d++)
                            {
                                secondCandidate[d] =
                                    candidate[d] +
                                    context.Random.NextDouble() *
                                    LevyStep(context.Random);
                            }

                            searchSpace.Clamp(secondCandidate);
                            double secondObjective =
                                context.Evaluate(secondCandidate, state);
                            RequireFinite(secondObjective);
                            rapidDiveEvaluations++;

                            if (problem.Sense.IsBetter(secondObjective, objectives[i]))
                            {
                                Array.Copy(secondCandidate, hawks[i], dimension);
                                objectives[i] = secondObjective;
                            }
                        }
                    }
                }

                state = state with
                {
                    RapidDiveEvaluations = rapidDiveEvaluations
                };

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            rabbitIndex = BestIndex(objectives, problem.Sense);

            state = new HarrisHawksOptimizerState(
                iteration,
                HarrisHawksOptimizerPhase.CompletedIteration,
                n,
                e1,
                rapidDiveEvaluations,
                objectives[rabbitIndex]);

            context.CompleteIteration(state.RabbitFitness, state);

            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumHarrisHawksIterations",
                "The configured HHO iteration limit was reached."),
            state);
    }

    private static double LevyStep(IRandomSource random)
    {
        double u = MantegnaSigmaBeta15 * StandardNormal(random);
        double v = StandardNormal(random);
        double denominator = Math.Pow(Math.Max(Math.Abs(v), 1e-15), 2.0 / 3.0);
        return u / denominator;
    }

    private static double StandardNormal(IRandomSource random)
    {
        double u1 = Math.Max(random.NextDouble(), 1e-15);
        double u2 = random.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    private static void MeanPosition(double[][] population, double[] destination)
    {
        Array.Clear(destination, 0, destination.Length);

        for (int i = 0; i < population.Length; i++)
        {
            for (int d = 0; d < destination.Length; d++)
                destination[d] += population[i][d];
        }

        for (int d = 0; d < destination.Length; d++)
            destination[d] /= population.Length;
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
            throw new InvalidOperationException("HHO requires finite objective values.");
    }
}
