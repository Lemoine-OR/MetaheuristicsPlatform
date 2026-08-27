using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Smpso;
public sealed class SmpsoOptimizer : IMultiobjectiveOptimizer<SmpsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.Smpso,
        Name = "Speed-Constrained Multiobjective PSO",
        Acronym = "SMPSO",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.SwarmIntelligence,
        Mechanisms = MetaheuristicMechanism.Adaptive,
        SearchSpaces = SearchSpaceKind.Continuous,
        IsStochastic = true,
        References = new[] { SmpsoReferences.NebroEtAl2009 }
    };
    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        SmpsoParameters parameters,
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
            MultiobjectiveToolkit.InsertArchive(archive, candidate, parameters.ArchiveSize, problem.ObjectiveSenses);
        foreach (MoCandidate candidate in swarm) candidate.Velocity = new double[dimension];
        ReadOnlySpan<double> lower = problem.SearchSpace.LowerBounds;
        ReadOnlySpan<double> upper = problem.SearchSpace.UpperBounds;
        for (int iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MultiobjectiveToolkit.AssignCrowding(archive, problem.ObjectiveSenses);
            for (int i = 0; i < swarm.Count; i++)
            {
                MoCandidate leader = SelectLeader(archive, random);
                double c1 = parameters.MinAcceleration +
                    (parameters.MaxAcceleration - parameters.MinAcceleration) * random.NextDouble();
                double c2 = parameters.MinAcceleration +
                    (parameters.MaxAcceleration - parameters.MinAcceleration) * random.NextDouble();
                double phi = c1 + c2;
                double chi = phi > 4.0
                    ? 2.0 / Math.Abs(2.0 - phi - Math.Sqrt(phi * phi - 4.0 * phi))
                    : 1.0;
                double[] velocity = swarm[i].Velocity!;
                for (int coordinate = 0; coordinate < dimension; coordinate++)
                {
                    double delta = 0.5 * (upper[coordinate] - lower[coordinate]);
                    velocity[coordinate] = chi * (
                        parameters.InertiaWeight * velocity[coordinate] +
                        c1 * random.NextDouble() * (personalBest[i].Position[coordinate] - swarm[i].Position[coordinate]) +
                        c2 * random.NextDouble() * (leader.Position[coordinate] - swarm[i].Position[coordinate]));
                    velocity[coordinate] = Math.Clamp(velocity[coordinate], -delta, delta);
                    swarm[i].Position[coordinate] += velocity[coordinate];
                }
                if (i % 3 == 0)
                    MultiobjectiveToolkit.PolynomialMutate(
                        swarm[i].Position, problem.SearchSpace, random,
                        1.0 / dimension, parameters.MutationDistributionIndex);
                problem.SearchSpace.Clamp(swarm[i].Position);
                MoCandidate evaluated = MultiobjectiveToolkit.Evaluate(
                    problem, (double[])swarm[i].Position.Clone(), ref evaluations);
                evaluated.Velocity = velocity;
                swarm[i] = evaluated;
                int comparison = ParetoDominance.Compare(
                    evaluated.Objectives, personalBest[i].Objectives, problem.ObjectiveSenses);
                if (comparison < 0 || (comparison == 0 && random.NextDouble() < 0.5))
                    personalBest[i] = MultiobjectiveToolkit.Clone(evaluated);
                MultiobjectiveToolkit.InsertArchive(archive, evaluated, parameters.ArchiveSize, problem.ObjectiveSenses);
            }
        }
        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(archive, problem.ObjectiveSenses),
            evaluations, parameters.MaximumIterations, seed);
    }
    private static MoCandidate SelectLeader(IReadOnlyList<MoCandidate> archive, IRandomSource random)
    {
        int first = random.NextInt32(archive.Count);
        int second = random.NextInt32(archive.Count);
        return archive[first].Crowding >= archive[second].Crowding
            ? archive[first]
            : archive[second];
    }
}
