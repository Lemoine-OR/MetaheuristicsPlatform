using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Parameters;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Multiobjective;

public delegate void ContinuousMultiobjectiveEvaluator(
    ReadOnlySpan<double> solution,
    Span<double> objectives);

public interface IContinuousMultiobjectiveOptimizationProblem
{
    IBoundedContinuousSearchSpace SearchSpace { get; }
    int ObjectiveCount { get; }
    IReadOnlyList<OptimizationSense> ObjectiveSenses { get; }
    void Evaluate(ReadOnlySpan<double> solution, Span<double> objectives);
}

public sealed class ContinuousMultiobjectiveOptimizationProblem :
    IContinuousMultiobjectiveOptimizationProblem
{
    private readonly OptimizationSense[] _senses;
    private readonly ContinuousMultiobjectiveEvaluator _evaluator;

    public ContinuousMultiobjectiveOptimizationProblem(
        IBoundedContinuousSearchSpace searchSpace,
        IReadOnlyList<OptimizationSense> objectiveSenses,
        ContinuousMultiobjectiveEvaluator evaluator)
    {
        ArgumentNullException.ThrowIfNull(searchSpace);
        ArgumentNullException.ThrowIfNull(objectiveSenses);
        ArgumentNullException.ThrowIfNull(evaluator);

        if (objectiveSenses.Count < 2)
            throw new ArgumentException(
                "A multiobjective problem requires at least two objectives.",
                nameof(objectiveSenses));

        SearchSpace = searchSpace;
        _senses = objectiveSenses.ToArray();
        _evaluator = evaluator;
    }

    public IBoundedContinuousSearchSpace SearchSpace { get; }
    public int ObjectiveCount => _senses.Length;
    public IReadOnlyList<OptimizationSense> ObjectiveSenses => _senses;

    public void Evaluate(ReadOnlySpan<double> solution, Span<double> objectives)
    {
        if (solution.Length != SearchSpace.Dimension)
            throw new ArgumentException(
                "Solution dimension does not match the search space.",
                nameof(solution));

        if (objectives.Length != ObjectiveCount)
            throw new ArgumentException(
                "Objective vector has the wrong dimension.",
                nameof(objectives));

        _evaluator(solution, objectives);

        for (int i = 0; i < objectives.Length; i++)
            if (!double.IsFinite(objectives[i]))
                throw new InvalidOperationException(
                    "Multiobjective evaluation must return finite objective values.");
    }
}

public sealed class MultiobjectivePoint
{
    public MultiobjectivePoint(double[] solution, double[] objectives)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(objectives);
        Solution = (double[])solution.Clone();
        Objectives = (double[])objectives.Clone();
    }

    public double[] Solution { get; }
    public double[] Objectives { get; }
}

public sealed class MultiobjectiveOptimizationResult
{
    public MultiobjectiveOptimizationResult(
        IReadOnlyList<MultiobjectivePoint> paretoFront,
        int evaluations,
        int iterations,
        ulong seed)
    {
        ArgumentNullException.ThrowIfNull(paretoFront);
        ParetoFront = paretoFront;
        Evaluations = evaluations;
        Iterations = iterations;
        Seed = seed;
    }

    public IReadOnlyList<MultiobjectivePoint> ParetoFront { get; }
    public int Evaluations { get; }
    public int Iterations { get; }
    public ulong Seed { get; }
}

public interface IMultiobjectiveOptimizer<in TParameters>
    where TParameters : IMetaheuristicParameters
{
    MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        TParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default);
}

public static class ParetoDominance
{
    public static int Compare(
        ReadOnlySpan<double> left,
        ReadOnlySpan<double> right,
        IReadOnlyList<OptimizationSense> senses)
    {
        ArgumentNullException.ThrowIfNull(senses);

        if (left.Length != right.Length ||
            left.Length != senses.Count)
            throw new ArgumentException(
                "Objective dimensions must agree.");

        bool leftBetter = false;
        bool rightBetter = false;

        for (int i = 0; i < left.Length; i++)
        {
            if (senses[i] == OptimizationSense.Minimize)
            {
                if (left[i] < right[i]) leftBetter = true;
                else if (right[i] < left[i]) rightBetter = true;
            }
            else
            {
                if (left[i] > right[i]) leftBetter = true;
                else if (right[i] > left[i]) rightBetter = true;
            }

            if (leftBetter && rightBetter)
                return 0;
        }

        if (leftBetter == rightBetter) return 0;
        return leftBetter ? -1 : 1;
    }
}

internal sealed class MoCandidate
{
    public MoCandidate(double[] position, double[] objectives)
    {
        Position = position;
        Objectives = objectives;
    }

    public double[] Position { get; }
    public double[] Objectives { get; }
    public int Rank { get; set; }
    public double Crowding { get; set; }
    public double Fitness { get; set; }
    public double[]? Velocity { get; set; }
}

internal static class MultiobjectiveToolkit
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

    public static MoCandidate Evaluate(
        IContinuousMultiobjectiveOptimizationProblem problem,
        double[] position,
        ref int evaluations)
    {
        double[] objectives = new double[problem.ObjectiveCount];
        problem.Evaluate(position, objectives);
        evaluations++;
        return new MoCandidate(position, objectives);
    }

    public static List<MoCandidate> Initialize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        int size,
        IRandomSource random,
        ref int evaluations)
    {
        List<MoCandidate> population = new(size);
        int dimension = problem.SearchSpace.Dimension;

        for (int i = 0; i < size; i++)
        {
            double[] x = new double[dimension];
            problem.SearchSpace.Sample(random, x);
            population.Add(Evaluate(problem, x, ref evaluations));
        }

        return population;
    }

    public static List<List<MoCandidate>> SortFronts(
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses)
    {
        int n = population.Count;
        List<int>[] dominates = new List<int>[n];
        int[] dominatedBy = new int[n];

        for (int i = 0; i < n; i++)
            dominates[i] = new List<int>();

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                int comparison = ParetoDominance.Compare(
                    population[i].Objectives,
                    population[j].Objectives,
                    senses);

                if (comparison < 0)
                {
                    dominates[i].Add(j);
                    dominatedBy[j]++;
                }
                else if (comparison > 0)
                {
                    dominates[j].Add(i);
                    dominatedBy[i]++;
                }
            }
        }

        List<List<MoCandidate>> fronts = new();
        List<int> current = new();

        for (int i = 0; i < n; i++)
            if (dominatedBy[i] == 0)
                current.Add(i);

        int rank = 0;

        while (current.Count > 0)
        {
            List<MoCandidate> front = new(current.Count);
            List<int> next = new();

            foreach (int index in current)
            {
                population[index].Rank = rank;
                front.Add(population[index]);

                foreach (int dominated in dominates[index])
                {
                    dominatedBy[dominated]--;
                    if (dominatedBy[dominated] == 0)
                        next.Add(dominated);
                }
            }

            AssignCrowding(front, senses);
            fronts.Add(front);
            current = next;
            rank++;
        }

        return fronts;
    }

    public static void AssignCrowding(
        IList<MoCandidate> front,
        IReadOnlyList<OptimizationSense> senses)
    {
        if (front.Count == 0)
            return;

        foreach (MoCandidate candidate in front)
            candidate.Crowding = 0.0;

        if (front.Count <= 2)
        {
            foreach (MoCandidate candidate in front)
                candidate.Crowding = double.PositiveInfinity;
            return;
        }

        int objectiveCount = front[0].Objectives.Length;

        for (int objective = 0; objective < objectiveCount; objective++)
        {
            int index = objective;
            List<MoCandidate> ordered =
                front.OrderBy(
                    candidate => Normalize(
                        candidate.Objectives[index],
                        senses[index]))
                    .ToList();

            ordered[0].Crowding = double.PositiveInfinity;
            ordered[^1].Crowding = double.PositiveInfinity;

            double minimum = Normalize(
                ordered[0].Objectives[index],
                senses[index]);

            double maximum = Normalize(
                ordered[^1].Objectives[index],
                senses[index]);

            double span = maximum - minimum;
            if (span <= 0.0)
                continue;

            for (int i = 1; i < ordered.Count - 1; i++)
            {
                if (double.IsPositiveInfinity(ordered[i].Crowding))
                    continue;

                double previous = Normalize(
                    ordered[i - 1].Objectives[index],
                    senses[index]);

                double next = Normalize(
                    ordered[i + 1].Objectives[index],
                    senses[index]);

                ordered[i].Crowding += (next - previous) / span;
            }
        }
    }

    public static List<MoCandidate> NsgaEnvironmentalSelection(
        IReadOnlyList<MoCandidate> candidates,
        int size,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<MoCandidate> selected = new(size);

        foreach (List<MoCandidate> front in SortFronts(candidates, senses))
        {
            if (selected.Count + front.Count <= size)
            {
                selected.AddRange(front);
                continue;
            }

            selected.AddRange(
                front.OrderByDescending(candidate => candidate.Crowding)
                    .Take(size - selected.Count));

            break;
        }

        SortFronts(selected, senses);
        return selected;
    }

    public static MoCandidate Tournament(
        IReadOnlyList<MoCandidate> population,
        IRandomSource random)
    {
        MoCandidate first =
            population[random.NextInt32(population.Count)];

        MoCandidate second =
            population[random.NextInt32(population.Count)];

        if (first.Rank != second.Rank)
            return first.Rank < second.Rank ? first : second;

        if (first.Crowding != second.Crowding)
            return first.Crowding > second.Crowding ? first : second;

        return random.NextDouble() < 0.5 ? first : second;
    }

    public static double[] SbxChild(
        ReadOnlySpan<double> firstParent,
        ReadOnlySpan<double> secondParent,
        IBoundedContinuousSearchSpace space,
        IRandomSource random,
        double crossoverProbability,
        double distributionIndex)
    {
        double[] child = firstParent.ToArray();

        if (random.NextDouble() > crossoverProbability)
            return child;

        ReadOnlySpan<double> lower = space.LowerBounds;
        ReadOnlySpan<double> upper = space.UpperBounds;

        for (int coordinate = 0;
             coordinate < child.Length;
             coordinate++)
        {
            if (random.NextDouble() > 0.5 ||
                Math.Abs(
                    firstParent[coordinate] -
                    secondParent[coordinate]) <= 1e-14)
                continue;

            double y1 = Math.Min(
                firstParent[coordinate],
                secondParent[coordinate]);

            double y2 = Math.Max(
                firstParent[coordinate],
                secondParent[coordinate]);

            double draw = random.NextDouble();

            double beta =
                1.0 +
                2.0 *
                (y1 - lower[coordinate]) /
                (y2 - y1);

            double alpha =
                2.0 -
                Math.Pow(
                    beta,
                    -(distributionIndex + 1.0));

            double betaQ =
                draw <= 1.0 / alpha
                    ? Math.Pow(
                        draw * alpha,
                        1.0 / (distributionIndex + 1.0))
                    : Math.Pow(
                        1.0 / (2.0 - draw * alpha),
                        1.0 / (distributionIndex + 1.0));

            double firstChild =
                0.5 *
                ((y1 + y2) -
                 betaQ * (y2 - y1));

            beta =
                1.0 +
                2.0 *
                (upper[coordinate] - y2) /
                (y2 - y1);

            alpha =
                2.0 -
                Math.Pow(
                    beta,
                    -(distributionIndex + 1.0));

            betaQ =
                draw <= 1.0 / alpha
                    ? Math.Pow(
                        draw * alpha,
                        1.0 / (distributionIndex + 1.0))
                    : Math.Pow(
                        1.0 / (2.0 - draw * alpha),
                        1.0 / (distributionIndex + 1.0));

            double secondChild =
                0.5 *
                ((y1 + y2) +
                 betaQ * (y2 - y1));

            firstChild =
                Math.Clamp(
                    firstChild,
                    lower[coordinate],
                    upper[coordinate]);

            secondChild =
                Math.Clamp(
                    secondChild,
                    lower[coordinate],
                    upper[coordinate]);

            child[coordinate] =
                random.NextDouble() < 0.5
                    ? firstChild
                    : secondChild;
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

        for (int coordinate = 0;
             coordinate < position.Length;
             coordinate++)
        {
            if (random.NextDouble() > probability)
                continue;

            double width =
                upper[coordinate] -
                lower[coordinate];

            if (width <= 0.0)
                continue;

            double delta1 =
                (position[coordinate] -
                 lower[coordinate]) /
                width;

            double delta2 =
                (upper[coordinate] -
                 position[coordinate]) /
                width;

            double draw = random.NextDouble();
            double mutationPower =
                1.0 /
                (distributionIndex + 1.0);

            double delta;

            if (draw <= 0.5)
            {
                double xy = 1.0 - delta1;
                double value =
                    2.0 * draw +
                    (1.0 - 2.0 * draw) *
                    Math.Pow(
                        xy,
                        distributionIndex + 1.0);

                delta =
                    Math.Pow(
                        value,
                        mutationPower) -
                    1.0;
            }
            else
            {
                double xy = 1.0 - delta2;
                double value =
                    2.0 * (1.0 - draw) +
                    2.0 * (draw - 0.5) *
                    Math.Pow(
                        xy,
                        distributionIndex + 1.0);

                delta =
                    1.0 -
                    Math.Pow(
                        value,
                        mutationPower);
            }

            position[coordinate] =
                Math.Clamp(
                    position[coordinate] +
                    delta * width,
                    lower[coordinate],
                    upper[coordinate]);
        }
    }

    public static bool InsertArchive(
        List<MoCandidate> archive,
        MoCandidate candidate,
        int capacity,
        IReadOnlyList<OptimizationSense> senses)
    {
        for (int i = archive.Count - 1; i >= 0; i--)
        {
            int comparison =
                ParetoDominance.Compare(
                    candidate.Objectives,
                    archive[i].Objectives,
                    senses);

            if (comparison > 0)
                return false;

            if (comparison < 0)
                archive.RemoveAt(i);
            else if (SameObjectives(
                         candidate.Objectives,
                         archive[i].Objectives))
                return false;
        }

        archive.Add(Clone(candidate));

        if (archive.Count > capacity)
        {
            AssignCrowding(archive, senses);

            int removeIndex =
                Enumerable.Range(0, archive.Count)
                    .OrderBy(
                        index =>
                            archive[index].Crowding)
                    .First();

            archive.RemoveAt(removeIndex);
        }

        return true;
    }

    public static bool InsertGridArchive(
        List<MoCandidate> archive,
        MoCandidate candidate,
        int capacity,
        int divisions,
        IReadOnlyList<OptimizationSense> senses,
        IRandomSource random)
    {
        for (int i = archive.Count - 1; i >= 0; i--)
        {
            int comparison =
                ParetoDominance.Compare(
                    candidate.Objectives,
                    archive[i].Objectives,
                    senses);

            if (comparison > 0)
                return false;

            if (comparison < 0)
                archive.RemoveAt(i);
            else if (SameObjectives(
                         candidate.Objectives,
                         archive[i].Objectives))
                return false;
        }

        archive.Add(Clone(candidate));

        while (archive.Count > capacity)
        {
            GridSnapshot grid =
                BuildGrid(
                    archive,
                    divisions,
                    senses);

            int maximumDensity =
                grid.Densities.Values.Max();

            int[] crowdedCells =
                grid.Densities
                    .Where(
                        pair =>
                            pair.Value ==
                            maximumDensity)
                    .Select(pair => pair.Key)
                    .ToArray();

            int chosenCell =
                crowdedCells[
                    random.NextInt32(
                        crowdedCells.Length)];

            int[] occupants =
                Enumerable.Range(
                        0,
                        archive.Count)
                    .Where(
                        index =>
                            grid.CellIds[index] ==
                            chosenCell)
                    .ToArray();

            archive.RemoveAt(
                occupants[
                    random.NextInt32(
                        occupants.Length)]);
        }

        return true;
    }

    public static MoCandidate SelectAdaptiveGridLeader(
        IReadOnlyList<MoCandidate> archive,
        int divisions,
        IReadOnlyList<OptimizationSense> senses,
        IRandomSource random)
    {
        if (archive.Count == 0)
            throw new InvalidOperationException(
                "Adaptive-grid archive cannot be empty.");

        GridSnapshot grid =
            BuildGrid(
                archive,
                divisions,
                senses);

        int[] occupiedCells =
            grid.Densities.Keys
                .OrderBy(value => value)
                .ToArray();

        double[] weights =
            new double[occupiedCells.Length];

        double total = 0.0;

        for (int i = 0;
             i < occupiedCells.Length;
             i++)
        {
            weights[i] =
                1.0 /
                grid.Densities[
                    occupiedCells[i]];

            total += weights[i];
        }

        double draw =
            random.NextDouble() *
            total;

        int selectedCell =
            occupiedCells[^1];

        for (int i = 0;
             i < occupiedCells.Length;
             i++)
        {
            draw -= weights[i];

            if (draw <= 0.0)
            {
                selectedCell =
                    occupiedCells[i];
                break;
            }
        }

        int[] members =
            Enumerable.Range(
                    0,
                    archive.Count)
                .Where(
                    index =>
                        grid.CellIds[index] ==
                        selectedCell)
                .ToArray();

        return archive[
            members[
                random.NextInt32(
                    members.Length)]];
    }

    public static MoCandidate Clone(MoCandidate source)
    {
        return new MoCandidate(
            (double[])source.Position.Clone(),
            (double[])source.Objectives.Clone())
        {
            Rank = source.Rank,
            Crowding = source.Crowding,
            Fitness = source.Fitness,
            Velocity =
                source.Velocity is null
                    ? null
                    : (double[])source.Velocity.Clone()
        };
    }

    public static List<MultiobjectivePoint> ResultFront(
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<List<MoCandidate>> fronts =
            SortFronts(
                population,
                senses);

        if (fronts.Count == 0)
            return new List<MultiobjectivePoint>();

        return fronts[0]
            .Select(
                candidate =>
                    new MultiobjectivePoint(
                        candidate.Position,
                        candidate.Objectives))
            .ToList();
    }

    public static double Normalize(
        double value,
        OptimizationSense sense)
    {
        return sense == OptimizationSense.Minimize
            ? value
            : -value;
    }

    public static double Tchebycheff(
        ReadOnlySpan<double> objectives,
        ReadOnlySpan<double> weights,
        ReadOnlySpan<double> ideal,
        IReadOnlyList<OptimizationSense> senses)
    {
        double worst =
            double.NegativeInfinity;

        for (int i = 0;
             i < objectives.Length;
             i++)
        {
            double normalized =
                Normalize(
                    objectives[i],
                    senses[i]);

            double weight =
                weights[i] <= 1e-12
                    ? 1e-6
                    : weights[i];

            worst =
                Math.Max(
                    worst,
                    weight *
                    Math.Abs(
                        normalized -
                        ideal[i]));
        }

        return worst;
    }

    public static int GridDensity(
        IReadOnlyList<MoCandidate> archive,
        MoCandidate candidate,
        int divisions,
        IReadOnlyList<OptimizationSense> senses)
    {
        GridSnapshot grid =
            BuildGrid(
                archive,
                divisions,
                senses);

        int candidateIndex = -1;

        for (int i = 0; i < archive.Count; i++)
            if (ReferenceEquals(
                    archive[i],
                    candidate))
            {
                candidateIndex = i;
                break;
            }

        if (candidateIndex < 0)
            return 0;

        return grid.Densities[
            grid.CellIds[
                candidateIndex]];
    }

    private sealed class GridSnapshot
    {
        public int[] CellIds { get; init; } =
            Array.Empty<int>();

        public Dictionary<int, int> Densities { get; init; } =
            new();
    }

    private static GridSnapshot BuildGrid(
        IReadOnlyList<MoCandidate> archive,
        int divisions,
        IReadOnlyList<OptimizationSense> senses)
    {
        if (archive.Count == 0)
            return new GridSnapshot();

        int objectives = senses.Count;
        double[] minimum = new double[objectives];
        double[] maximum = new double[objectives];

        for (int objective = 0;
             objective < objectives;
             objective++)
        {
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;

            for (int i = 0;
                 i < archive.Count;
                 i++)
            {
                double value =
                    Normalize(
                        archive[i].Objectives[objective],
                        senses[objective]);

                if (value < min) min = value;
                if (value > max) max = value;
            }

            double span = max - min;
            double margin =
                span <= 1e-12
                    ? 1.0
                    : 0.1 * span;

            minimum[objective] =
                min - margin;

            maximum[objective] =
                max + margin;
        }

        int[] ids =
            new int[archive.Count];

        Dictionary<int, int> densities =
            new();

        for (int i = 0;
             i < archive.Count;
             i++)
        {
            int id = 0;
            int multiplier = 1;

            for (int objective = 0;
                 objective < objectives;
                 objective++)
            {
                int cell =
                    Cell(
                        Normalize(
                            archive[i].Objectives[objective],
                            senses[objective]),
                        minimum[objective],
                        maximum[objective],
                        divisions);

                id +=
                    multiplier *
                    cell;

                multiplier *= divisions;
            }

            ids[i] = id;

            if (densities.TryGetValue(
                    id,
                    out int count))
                densities[id] =
                    count + 1;
            else
                densities[id] = 1;
        }

        return new GridSnapshot
        {
            CellIds = ids,
            Densities = densities
        };
    }

    private static int Cell(
        double value,
        double minimum,
        double maximum,
        int divisions)
    {
        if (maximum <= minimum)
            return 0;

        double scaled =
            (value - minimum) /
            (maximum - minimum);

        return Math.Clamp(
            (int)Math.Floor(
                scaled * divisions),
            0,
            divisions - 1);
    }

    private static bool SameObjectives(
        ReadOnlySpan<double> first,
        ReadOnlySpan<double> second)
    {
        if (first.Length != second.Length)
            return false;

        for (int i = 0;
             i < first.Length;
             i++)
            if (Math.Abs(
                    first[i] -
                    second[i]) > 1e-12)
                return false;

        return true;
    }
}
