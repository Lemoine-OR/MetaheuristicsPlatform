using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Mopso;
public sealed class MopsoOptimizer : IMultiobjectiveOptimizer<MopsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.Mopso,
        Name = "Multiobjective Particle Swarm Optimizer",
        Acronym = "MOPSO",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.SwarmIntelligence,
        Mechanisms = MetaheuristicMechanism.Adaptive,
        SearchSpaces = SearchSpaceKind.Continuous,
        IsStochastic = true,
        References = new[] { MopsoReferences.CoelloPulidoLechuga2004 }
    };
    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        MopsoParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();
        IRandomSource random = MultiobjectiveToolkit.CreateRandom(options, out ulong seed);
        int evaluations = 0;
        int dimension = problem.SearchSpace.Dimension;
        List<MoCandidate> swarm = MultiobjectiveToolkit.Initialize(problem, parameters.SwarmSize, random, ref evaluations);
        List<MoCandidate> personalBest = swarm.Select(MultiobjectiveToolkit.Clone).ToList();
        List<MoCandidate> archive = new();
        foreach (MoCandidate candidate in swarm)
            MultiobjectiveToolkit.InsertGridArchive(archive, candidate, parameters.ArchiveSize, parameters.GridDivisions, problem.ObjectiveSenses, random);
        foreach (MoCandidate candidate in swarm) candidate.Velocity = new double[dimension];
        for (int iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int i = 0; i < swarm.Count; i++)
            {
                MoCandidate leader = MultiobjectiveToolkit.SelectAdaptiveGridLeader(
                    archive, parameters.GridDivisions, problem.ObjectiveSenses, random);
                double[] velocity = swarm[i].Velocity!;
                for (int coordinate = 0; coordinate < dimension; coordinate++)
                {
                    velocity[coordinate] =
                        parameters.InertiaWeight * velocity[coordinate] +
                        random.NextDouble() * (personalBest[i].Position[coordinate] - swarm[i].Position[coordinate]) +
                        random.NextDouble() * (leader.Position[coordinate] - swarm[i].Position[coordinate]);
                    swarm[i].Position[coordinate] += velocity[coordinate];
                }
                Mutate(swarm[i].Position, problem.SearchSpace, random, iteration, parameters.MaximumIterations, parameters.MutationRate);
                ReflectBounds(swarm[i].Position, velocity, problem.SearchSpace);
                MoCandidate evaluated = MultiobjectiveToolkit.Evaluate(
                    problem, (double[])swarm[i].Position.Clone(), ref evaluations);
                evaluated.Velocity = velocity;
                swarm[i] = evaluated;
                int comparison = ParetoDominance.Compare(
                    evaluated.Objectives, personalBest[i].Objectives, problem.ObjectiveSenses);
                if (comparison < 0 || (comparison == 0 && random.NextDouble() < 0.5))
                    personalBest[i] = MultiobjectiveToolkit.Clone(evaluated);
                MultiobjectiveToolkit.InsertGridArchive(
                    archive, evaluated, parameters.ArchiveSize, parameters.GridDivisions, problem.ObjectiveSenses, random);
            }
        }
        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(archive, problem.ObjectiveSenses),
            evaluations, parameters.MaximumIterations, seed);
    }
    private static void Mutate(
        double[] position,
        IBoundedContinuousSearchSpace space,
        IRandomSource random,
        int iteration,
        int maximumIterations,
        double mutationRate)
    {
        double progress = iteration / (double)maximumIterations;
        double probability = Math.Pow(Math.Max(0.0, 1.0 - progress), 5.0 / mutationRate);
        if (random.NextDouble() > probability) return;
        int coordinate = random.NextInt32(position.Length);
        ReadOnlySpan<double> lower = space.LowerBounds;
        ReadOnlySpan<double> upper = space.UpperBounds;
        double radius = (upper[coordinate] - lower[coordinate]) * probability;
        double left = Math.Max(lower[coordinate], position[coordinate] - radius);
        double right = Math.Min(upper[coordinate], position[coordinate] + radius);
        position[coordinate] = left + (right - left) * random.NextDouble();
    }
    private static void ReflectBounds(
        double[] position,
        double[] velocity,
        IBoundedContinuousSearchSpace space)
    {
        ReadOnlySpan<double> lower = space.LowerBounds;
        ReadOnlySpan<double> upper = space.UpperBounds;
        for (int coordinate = 0; coordinate < position.Length; coordinate++)
        {
            if (position[coordinate] < lower[coordinate])
            {
                position[coordinate] = lower[coordinate];
                velocity[coordinate] = -velocity[coordinate];
            }
            else if (position[coordinate] > upper[coordinate])
            {
                position[coordinate] = upper[coordinate];
                velocity[coordinate] = -velocity[coordinate];
            }
        }
    }
}
