using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

public sealed class ConstrictionParticleSwarmOptimizer :
    IMetaheuristic<double[], ConstrictionPsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ConstrictionParticleSwarm,
            Name = "Clerc-Kennedy Constriction Particle Swarm",
            Acronym = "CKPSO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [ConstrictionPsoReferences.ClercKennedy2002]
        };

    public ConstrictionPsoParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        ConstrictionPsoParameters parameters,
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
            "Clerc-Kennedy-2002-constriction",
            problem,
            parameters.SwarmSize,
            parameters.MaximumIterations,
            parameters.CognitiveCoefficient,
            parameters.SocialCoefficient,
            parameters.InitialVelocityRangeFraction,
            new ClercKennedyConstrictionDynamics(parameters.Phi, parameters.Kappa),
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);
    }
}
