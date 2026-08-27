using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.MoCmaEs;

public sealed class MoCmaEsOptimizer :
    IMultiobjectiveOptimizer<MoCmaEsParameters>
{
    private sealed class StrategyState
    {
        public StrategyState(
            MoCandidate candidate,
            double[,] covariance,
            double sigma,
            double successProbability)
        {
            Candidate = candidate;
            Covariance = covariance;
            Sigma = sigma;
            SuccessProbability = successProbability;
        }

        public MoCandidate Candidate { get; }
        public double[,] Covariance { get; }
        public double Sigma { get; }
        public double SuccessProbability { get; }
    }

    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.MoCmaEs,
            Name = "Multi-objective CMA-ES",
            Acronym = "MO-CMA-ES",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { MoCmaEsReferences.IgelHansenRoth2007 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        MoCmaEsParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();

        IRandomSource random =
            MultiobjectiveToolkit.CreateRandom(
                options,
                out ulong seed);

        int evaluations = 0;
        int dimension = problem.SearchSpace.Dimension;

        List<MoCandidate> initial =
            MultiobjectiveToolkit.Initialize(
                problem,
                parameters.PopulationSize,
                random,
                ref evaluations);

        double averageRange = 0.0;

        for (int coordinate = 0; coordinate < dimension; coordinate++)
            averageRange +=
                problem.SearchSpace.UpperBounds[coordinate] -
                problem.SearchSpace.LowerBounds[coordinate];

        averageRange /=
            Math.Max(
                dimension,
                1);

        double initialSigma =
            parameters.InitialStepSizeFraction *
            Math.Max(
                averageRange,
                1e-12);

        List<StrategyState> states =
            initial
                .Select(
                    candidate =>
                        new StrategyState(
                            candidate,
                            Identity(dimension),
                            initialSigma,
                            parameters.SuccessTarget))
                .ToList();

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<StrategyState> offspring =
                new(states.Count);

            foreach (StrategyState parent in states)
            {
                double[,] factor =
                    MultiobjectiveAdvancedToolkit.Cholesky(
                        parent.Covariance);

                double[] normal =
                    new double[dimension];

                for (int coordinate = 0;
                     coordinate < dimension;
                     coordinate++)
                    normal[coordinate] =
                        MultiobjectiveAdvancedToolkit.NextGaussian(
                            random);

                double[] step =
                    new double[dimension];

                double[] child =
                    (double[])parent.Candidate.Position.Clone();

                for (int row = 0; row < dimension; row++)
                {
                    double value = 0.0;

                    for (int column = 0; column <= row; column++)
                        value +=
                            factor[row, column] *
                            normal[column];

                    step[row] =
                        parent.Sigma *
                        value;

                    child[row] += step[row];
                }

                problem.SearchSpace.Clamp(child);

                MoCandidate evaluated =
                    MultiobjectiveToolkit.Evaluate(
                        problem,
                        child,
                        ref evaluations);

                bool success =
                    ParetoDominance.Compare(
                        evaluated.Objectives,
                        parent.Candidate.Objectives,
                        problem.ObjectiveSenses) < 0;

                double successProbability =
                    0.9 *
                    parent.SuccessProbability +
                    0.1 *
                    (success ? 1.0 : 0.0);

                double sigma =
                    parent.Sigma *
                    Math.Exp(
                        (
                            successProbability -
                            parameters.SuccessTarget) /
                        (
                            parameters.StepSizeDamping *
                            Math.Max(
                                1.0 -
                                parameters.SuccessTarget,
                                1e-12)));

                double[,] covariance =
                    RankOneUpdate(
                        parent.Covariance,
                        step,
                        Math.Max(
                            parent.Sigma,
                            1e-12),
                        parameters.CovarianceLearningRate);

                offspring.Add(
                    new StrategyState(
                        evaluated,
                        covariance,
                        sigma,
                        successProbability));
            }

            List<StrategyState> union =
                new(states.Count + offspring.Count);

            union.AddRange(states);
            union.AddRange(offspring);

            List<MoCandidate> selected =
                MultiobjectiveToolkit.NsgaEnvironmentalSelection(
                    union
                        .Select(state => state.Candidate)
                        .ToList(),
                    parameters.PopulationSize,
                    problem.ObjectiveSenses);

            Dictionary<MoCandidate, StrategyState> map =
                union.ToDictionary(
                    state => state.Candidate,
                    state => state);

            states =
                selected
                    .Select(candidate => map[candidate])
                    .ToList();
        }

        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(
                states.Select(state => state.Candidate).ToList(),
                problem.ObjectiveSenses),
            evaluations,
            parameters.MaximumGenerations,
            seed);
    }

    private static double[,] Identity(int dimension)
    {
        double[,] matrix =
            new double[dimension, dimension];

        for (int i = 0; i < dimension; i++)
            matrix[i, i] = 1.0;

        return matrix;
    }

    private static double[,] RankOneUpdate(
        double[,] covariance,
        ReadOnlySpan<double> step,
        double sigma,
        double learningRate)
    {
        int dimension = step.Length;

        double[,] result =
            new double[dimension, dimension];

        for (int row = 0; row < dimension; row++)
            for (int column = 0; column < dimension; column++)
            {
                double normalizedOuter =
                    (step[row] / sigma) *
                    (step[column] / sigma);

                result[row, column] =
                    (1.0 - learningRate) *
                    covariance[row, column] +
                    learningRate *
                    normalizedOuter;
            }

        for (int i = 0; i < dimension; i++)
            result[i, i] =
                Math.Max(
                    result[i, i],
                    1e-12);

        return result;
    }
}
