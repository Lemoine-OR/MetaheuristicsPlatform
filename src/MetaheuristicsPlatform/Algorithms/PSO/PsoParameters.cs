using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.PSO;

/// <summary>
/// Strongly typed parameters for continuous Particle Swarm Optimization.
/// </summary>
public sealed class PsoParameters :
    IMetaheuristicParameters
{
    private PsoExecutionOptions _movementExecution =
        new();

    public int SwarmSize { get; init; } = 40;

    public IPsoTopology Topology { get; init; } =
        new FullyConnectedTopology();

    public IPsoInfluencePolicy InfluencePolicy { get; init; } =
        new CanonicalBestInfluencePolicy(
            cognitiveCoefficient: 2.05,
            socialCoefficient: 2.05);

    public IPsoVelocityDynamics VelocityDynamics { get; init; } =
        new ClercKennedyConstrictionDynamics(
            phi: 4.10);

    public double InitialVelocityRangeFraction { get; init; } =
        0.5;

    public double? VelocityLimitRangeFraction { get; init; } =
        1.0;

    public PsoBoundaryHandling BoundaryHandling { get; init; } =
        PsoBoundaryHandling.Clamp;

    /// <summary>
    /// Execution controls for homogeneous PSO movement/social kernels.
    /// </summary>
    public PsoExecutionOptions MovementExecution
    {
        get => _movementExecution;
        init =>
            _movementExecution =
                value ??
                throw new ArgumentNullException(
                    nameof(value));
    }

    /// <summary>
    /// Backward-compatible alias for MovementExecution.
    /// </summary>
    public PsoExecutionOptions Execution
    {
        get => _movementExecution;
        init =>
            _movementExecution =
                value ??
                throw new ArgumentNullException(
                    nameof(value));
    }

    /// <summary>
    /// Generic objective/candidate evaluation execution policy.
    /// </summary>
    public EvaluationExecutionOptions EvaluationExecution { get; init; } =
        new();

    /// <summary>
    /// Compatibility master switch. False forces sequential objective evaluation.
    /// </summary>
    public bool EnableParallelObjectiveEvaluation { get; init; } =
        true;

    public void Validate()
    {
        if (SwarmSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SwarmSize));
        }

        ArgumentNullException.ThrowIfNull(Topology);
        ArgumentNullException.ThrowIfNull(InfluencePolicy);
        ArgumentNullException.ThrowIfNull(VelocityDynamics);
        ArgumentNullException.ThrowIfNull(MovementExecution);
        ArgumentNullException.ThrowIfNull(EvaluationExecution);

        if (!double.IsFinite(
                InitialVelocityRangeFraction) ||
            InitialVelocityRangeFraction < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(InitialVelocityRangeFraction));
        }

        if (VelocityLimitRangeFraction.HasValue &&
            (!double.IsFinite(
                 VelocityLimitRangeFraction.Value) ||
             VelocityLimitRangeFraction.Value <= 0.0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(VelocityLimitRangeFraction));
        }

        MovementExecution.Validate();
        EvaluationExecution.Validate();
    }
}