using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

public sealed class InertiaWeightParticleSwarmOptimizer :
    IMetaheuristic<double[], InertiaWeightPsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.InertiaWeightParticleSwarm,
            Name = "Inertia Weight Particle Swarm Optimization",
            Acronym = "IWPSO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [InertiaWeightPsoReferences.ShiEberhart1998]
        };

    public InertiaWeightPsoParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        InertiaWeightPsoParameters parameters,
        ISolutionCloner<double[]> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<double[]>? callback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();

        return ScientificCanonicalPsoRunner.Optimize(
            Descriptor,
            "Shi-Eberhart-1998-constant-inertia",
            problem,
            parameters.SwarmSize,
            parameters.MaximumIterations,
            parameters.CognitiveCoefficient,
            parameters.SocialCoefficient,
            parameters.InitialVelocityRangeFraction,
            new ConstantInertiaDynamics(parameters.InertiaWeight),
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);
    }
}
