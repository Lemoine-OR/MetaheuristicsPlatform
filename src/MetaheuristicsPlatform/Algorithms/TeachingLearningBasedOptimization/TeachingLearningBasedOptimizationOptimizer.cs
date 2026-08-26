using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.TeachingLearningBasedOptimization;

public sealed class TeachingLearningBasedOptimizationOptimizer :
    IMetaheuristic<double[], TeachingLearningBasedOptimizationParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.TeachingLearningBasedOptimization,
            Name = "Teaching-Learning-Based Optimization",
            Acronym = "TLBO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [TeachingLearningBasedOptimizationReferences.RaoSavsaniVakharia2011]
        };

    public TeachingLearningBasedOptimizationParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        TeachingLearningBasedOptimizationParameters parameters,
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
            throw new NotSupportedException("TLBO requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        if (dimension <= 0)
            throw new InvalidOperationException("TLBO requires a positive dimension.");

        int n = parameters.PopulationSize;
        double[][] learners = CreatePopulation(n, dimension);
        double[] objectives = new double[n];
        double[] teacher = new double[dimension];
        double[] mean = new double[dimension];
        double[] candidate = new double[dimension];

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new TeachingLearningBasedOptimizationState(
            0, TeachingLearningBasedOptimizationPhase.Initialization, n, null);
        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, learners[i]);
            objectives[i] = context.Evaluate(learners[i], state);
            RequireFinite(objectives[i]);
            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ComputeMean(learners, mean);
            int teacherIndex = BestIndex(objectives, problem.Sense);
            Array.Copy(learners[teacherIndex], teacher, dimension);

            state = new TeachingLearningBasedOptimizationState(
                iteration - 1,
                TeachingLearningBasedOptimizationPhase.Teacher,
                n,
                objectives[teacherIndex]);

            for (int i = 0; i < n; i++)
            {
                int teachingFactor = context.Random.NextDouble() < 0.5 ? 1 : 2;
                double r = context.Random.NextDouble();
                for (int d = 0; d < dimension; d++)
                    candidate[d] = learners[i][d] + r * (teacher[d] - teachingFactor * mean[d]);

                searchSpace.Clamp(candidate);
                double candidateObjective = context.Evaluate(candidate, state);
                RequireFinite(candidateObjective);

                if (problem.Sense.IsBetter(candidateObjective, objectives[i]))
                {
                    Array.Copy(candidate, learners[i], dimension);
                    objectives[i] = candidateObjective;
                }

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            state = state with { Phase = TeachingLearningBasedOptimizationPhase.Learner };

            for (int i = 0; i < n; i++)
            {
                int partner;
                do { partner = context.Random.NextInt32(n); } while (partner == i);

                double r = context.Random.NextDouble();
                bool iIsBetter = problem.Sense.IsBetter(objectives[i], objectives[partner]);
                for (int d = 0; d < dimension; d++)
                {
                    double direction = iIsBetter
                        ? learners[i][d] - learners[partner][d]
                        : learners[partner][d] - learners[i][d];
                    candidate[d] = learners[i][d] + r * direction;
                }

                searchSpace.Clamp(candidate);
                double candidateObjective = context.Evaluate(candidate, state);
                RequireFinite(candidateObjective);

                if (problem.Sense.IsBetter(candidateObjective, objectives[i]))
                {
                    Array.Copy(candidate, learners[i], dimension);
                    objectives[i] = candidateObjective;
                }

                StoppingDecision stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            double best = BestObjective(objectives, problem.Sense);
            state = new TeachingLearningBasedOptimizationState(
                iteration,
                TeachingLearningBasedOptimizationPhase.CompletedIteration,
                n,
                best);

            context.CompleteIteration(best, state);
            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumTeachingLearningBasedOptimizationIterations",
                "The configured TLBO iteration limit was reached."),
            state);
    }

    private static double[][] CreatePopulation(int count, int dimension)
    {
        double[][] result = new double[count][];
        for (int i = 0; i < count; i++) result[i] = new double[dimension];
        return result;
    }

    private static void ComputeMean(double[][] population, Span<double> mean)
    {
        mean.Clear();
        for (int i = 0; i < population.Length; i++)
            for (int d = 0; d < mean.Length; d++)
                mean[d] += population[i][d];
        for (int d = 0; d < mean.Length; d++) mean[d] /= population.Length;
    }

    private static int BestIndex(ReadOnlySpan<double> values, OptimizationSense sense)
    {
        int best = 0;
        for (int i = 1; i < values.Length; i++)
            if (sense.IsBetter(values[i], values[best])) best = i;
        return best;
    }

    private static double BestObjective(ReadOnlySpan<double> values, OptimizationSense sense)
    {
        return values[BestIndex(values, sense)];
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
            throw new InvalidOperationException("TLBO requires finite objective values.");
    }
}
