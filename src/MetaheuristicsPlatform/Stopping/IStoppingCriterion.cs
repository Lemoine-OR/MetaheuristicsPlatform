using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Stopping;

/// <summary>
/// Common contract for generic and algorithm-specific stopping criteria.
/// </summary>
public interface IStoppingCriterion
{
    /// <summary>Gets a stable criterion name.</summary>
    string Name { get; }

    /// <summary>Evaluates the criterion against the common optimization state.</summary>
    StoppingDecision Evaluate(in OptimizationState state, OptimizationSense sense);
}