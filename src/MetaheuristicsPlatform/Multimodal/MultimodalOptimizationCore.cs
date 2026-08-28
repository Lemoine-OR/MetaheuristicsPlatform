using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Parameters;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Multimodal;

public delegate double ContinuousMultimodalEvaluator(
    ReadOnlySpan<double> solution);

public interface IContinuousMultimodalOptimizationProblem
{
    IBoundedContinuousSearchSpace SearchSpace { get; }
    OptimizationSense Sense { get; }
    double Evaluate(ReadOnlySpan<double> solution);
}

public sealed class ContinuousMultimodalOptimizationProblem :
    IContinuousMultimodalOptimizationProblem
{
    private readonly ContinuousMultimodalEvaluator _evaluator;

    public ContinuousMultimodalOptimizationProblem(
        IBoundedContinuousSearchSpace searchSpace,
        OptimizationSense sense,
        ContinuousMultimodalEvaluator evaluator)
    {
        ArgumentNullException.ThrowIfNull(searchSpace);
        ArgumentNullException.ThrowIfNull(evaluator);
        SearchSpace = searchSpace;
        Sense = sense;
        _evaluator = evaluator;
    }

    public IBoundedContinuousSearchSpace SearchSpace { get; }
    public OptimizationSense Sense { get; }

    public double Evaluate(ReadOnlySpan<double> solution)
    {
        if (solution.Length != SearchSpace.Dimension)
            throw new ArgumentException(
                "Solution dimension does not match the search space.",
                nameof(solution));

        double value = _evaluator(solution);

        if (!double.IsFinite(value))
            throw new InvalidOperationException(
                "Multimodal objective evaluation must be finite.");

        return value;
    }
}

public sealed class MultimodalPoint
{
    public MultimodalPoint(double[] solution, double objective)
    {
        ArgumentNullException.ThrowIfNull(solution);
        Solution = (double[])solution.Clone();
        Objective = objective;
    }

    public double[] Solution { get; }
    public double Objective { get; }
}

public sealed class MultimodalOptimizationResult
{
    public MultimodalOptimizationResult(
        IReadOnlyList<MultimodalPoint> optima,
        int evaluations,
        int iterations,
        ulong seed)
    {
        ArgumentNullException.ThrowIfNull(optima);
        Optima = optima;
        Evaluations = evaluations;
        Iterations = iterations;
        Seed = seed;
    }

    public IReadOnlyList<MultimodalPoint> Optima { get; }
    public int Evaluations { get; }
    public int Iterations { get; }
    public ulong Seed { get; }
}

public interface IMultimodalOptimizer<in TParameters>
    where TParameters : IMetaheuristicParameters
{
    MultimodalOptimizationResult Optimize(
        IContinuousMultimodalOptimizationProblem problem,
        TParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default);
}

internal sealed class MultimodalCandidate
{
    public MultimodalCandidate(double[] position, double objective)
    {
        Position = position;
        Objective = objective;
        PersonalBest = (double[])position.Clone();
        PersonalBestObjective = objective;
        Velocity = new double[position.Length];
    }

    public double[] Position { get; }
    public double Objective { get; set; }
    public double[] PersonalBest { get; }
    public double PersonalBestObjective { get; set; }
    public double[] Velocity { get; }
    public double Score { get; set; }
    public bool Cleared { get; set; }
}

internal static class MultimodalToolkit
{
    public static IRandomSource CreateRandom(
        OptimizationOptions? options,
        out ulong seed)
    {
        options ??= new OptimizationOptions();
        options.Validate();
        seed = options.Seed;
        return options.RandomSourceFactory.Create(seed);
    }

    public static double Key(
        double objective,
        OptimizationSense sense)
    {
        return sense == OptimizationSense.Minimize
            ? objective
            : -objective;
    }

    public static bool Better(
        double left,
        double right,
        OptimizationSense sense)
    {
        return Key(left, sense) < Key(right, sense);
    }

    public static MultimodalCandidate Evaluate(
        IContinuousMultimodalOptimizationProblem problem,
        double[] position,
        ref int evaluations)
    {
        double objective = problem.Evaluate(position);
        evaluations++;
        return new MultimodalCandidate(position, objective);
    }

    public static List<MultimodalCandidate> Initialize(
        IContinuousMultimodalOptimizationProblem problem,
        int size,
        IRandomSource random,
        ref int evaluations)
    {
        List<MultimodalCandidate> population = new(size);

        for (int i = 0; i < size; i++)
        {
            double[] position = new double[problem.SearchSpace.Dimension];
            problem.SearchSpace.Sample(random, position);
            population.Add(Evaluate(problem, position, ref evaluations));
        }

        return population;
    }

    public static double Distance(
        ReadOnlySpan<double> left,
        ReadOnlySpan<double> right)
    {
        double sum = 0.0;
        for (int i = 0; i < left.Length; i++)
        {
            double delta = left[i] - right[i];
            sum += delta * delta;
        }

        return Math.Sqrt(sum);
    }

    public static int ClosestIndex(
        IReadOnlyList<MultimodalCandidate> population,
        ReadOnlySpan<double> position)
    {
        int best = 0;
        double bestDistance =
            Distance(population[0].Position, position);

        for (int i = 1; i < population.Count; i++)
        {
            double distance =
                Distance(population[i].Position, position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }

        return best;
    }

    public static int[] NearestIndices(
        IReadOnlyList<MultimodalCandidate> population,
        int targetIndex,
        int count)
    {
        return Enumerable.Range(0, population.Count)
            .Where(index => index != targetIndex)
            .OrderBy(index =>
                Distance(
                    population[targetIndex].Position,
                    population[index].Position))
            .Take(Math.Min(count, population.Count - 1))
            .ToArray();
    }

    public static double MedianNearestNeighborDistance(
        IReadOnlyList<MultimodalCandidate> population)
    {
        if (population.Count < 2)
            return 0.0;

        double[] nearest = new double[population.Count];

        for (int i = 0; i < population.Count; i++)
        {
            double best = double.PositiveInfinity;

            for (int j = 0; j < population.Count; j++)
            {
                if (i == j)
                    continue;

                best = Math.Min(
                    best,
                    Distance(
                        population[i].Position,
                        population[j].Position));
            }

            nearest[i] = best;
        }

        Array.Sort(nearest);
        return nearest[nearest.Length / 2];
    }

    public static double[] SbxChild(
        ReadOnlySpan<double> first,
        ReadOnlySpan<double> second,
        IBoundedContinuousSearchSpace space,
        IRandomSource random,
        double crossoverProbability,
        double distributionIndex)
    {
        double[] child = first.ToArray();

        if (random.NextDouble() > crossoverProbability)
            return child;

        ReadOnlySpan<double> lower = space.LowerBounds;
        ReadOnlySpan<double> upper = space.UpperBounds;

        for (int i = 0; i < child.Length; i++)
        {
            if (random.NextDouble() > 0.5)
                continue;

            double u = random.NextDouble();
            double beta =
                u <= 0.5
                    ? Math.Pow(
                        2.0 * u,
                        1.0 / (distributionIndex + 1.0))
                    : Math.Pow(
                        1.0 / (2.0 * (1.0 - u)),
                        1.0 / (distributionIndex + 1.0));

            child[i] =
                Math.Clamp(
                    0.5 *
                    ((1.0 + beta) * first[i] +
                     (1.0 - beta) * second[i]),
                    lower[i],
                    upper[i]);
        }

        return child;
    }

    public static void PolynomialMutate(
        Span<double> position,
        IBoundedContinuousSearchSpace space,
        IRandomSource random,
        double probability,
        double distributionIndex)
    {
        ReadOnlySpan<double> lower = space.LowerBounds;
        ReadOnlySpan<double> upper = space.UpperBounds;

        for (int i = 0; i < position.Length; i++)
        {
            if (random.NextDouble() > probability)
                continue;

            double width = upper[i] - lower[i];
            if (width <= 0.0)
                continue;

            double u = random.NextDouble();
            double delta =
                u < 0.5
                    ? Math.Pow(
                        2.0 * u,
                        1.0 / (distributionIndex + 1.0)) - 1.0
                    : 1.0 -
                      Math.Pow(
                        2.0 * (1.0 - u),
                        1.0 / (distributionIndex + 1.0));

            position[i] =
                Math.Clamp(
                    position[i] + delta * width,
                    lower[i],
                    upper[i]);
        }
    }

    public static double[] DifferentialTrial(
        IReadOnlyList<MultimodalCandidate> population,
        int targetIndex,
        IReadOnlyList<int> pool,
        IBoundedContinuousSearchSpace space,
        IRandomSource random,
        double differentialWeight,
        double crossoverProbability)
    {
        int[] available =
            pool
                .Where(index => index != targetIndex)
                .Distinct()
                .ToArray();

        if (available.Length < 3)
            throw new ArgumentException(
                "Differential mutation requires at least three donor indices distinct from the target.",
                nameof(pool));

        int a =
            available[
                random.NextInt32(
                    available.Length)];

        int b;
        int c;

        do
        {
            b =
                available[
                    random.NextInt32(
                        available.Length)];
        }
        while (b == a);

        do
        {
            c =
                available[
                    random.NextInt32(
                        available.Length)];
        }
        while (c == a || c == b);

        double[] trial =
            (double[])population[targetIndex].Position.Clone();

        int forced = random.NextInt32(trial.Length);
        ReadOnlySpan<double> lower = space.LowerBounds;
        ReadOnlySpan<double> upper = space.UpperBounds;

        for (int d = 0; d < trial.Length; d++)
        {
            if (d != forced &&
                random.NextDouble() > crossoverProbability)
                continue;

            trial[d] =
                population[a].Position[d] +
                differentialWeight *
                (population[b].Position[d] -
                 population[c].Position[d]);

            trial[d] =
                Math.Clamp(trial[d], lower[d], upper[d]);
        }

        return trial;
    }

    public static List<MultimodalPoint> ExtractDistinctOptima(
        IReadOnlyList<MultimodalCandidate> population,
        OptimizationSense sense,
        double nicheRadius,
        int maximumOptima)
    {
        List<MultimodalPoint> optima = new();

        foreach (MultimodalCandidate candidate in
            population.OrderBy(c => Key(c.Objective, sense)))
        {
            bool distinct =
                optima.All(point =>
                    Distance(
                        point.Solution,
                        candidate.Position) > nicheRadius);

            if (!distinct)
                continue;

            optima.Add(
                new MultimodalPoint(
                    candidate.Position,
                    candidate.Objective));

            if (optima.Count >= maximumOptima)
                break;
        }

        return optima;
    }
}
