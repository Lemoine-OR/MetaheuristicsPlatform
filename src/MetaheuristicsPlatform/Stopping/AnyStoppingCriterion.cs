using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Stopping;

/// <summary>Stops as soon as any child criterion stops.</summary>
public sealed class AnyStoppingCriterion : IStoppingCriterion
{
    private readonly IReadOnlyList<IStoppingCriterion> _criteria;

    public AnyStoppingCriterion(params IStoppingCriterion[] criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        if (criteria.Length == 0)
        {
            throw new ArgumentException("At least one stopping criterion is required.", nameof(criteria));
        }

        if (criteria.Any(static criterion => criterion is null))
        {
            throw new ArgumentException("Stopping criteria cannot contain null elements.", nameof(criteria));
        }

        _criteria = criteria;
    }

    public string Name => "Any";

    public StoppingDecision Evaluate(in OptimizationState state, OptimizationSense sense)
    {
        foreach (IStoppingCriterion criterion in _criteria)
        {
            StoppingDecision decision = criterion.Evaluate(in state, sense);
            if (decision.ShouldStop)
            {
                return decision;
            }
        }

        return StoppingDecision.Continue(Name);
    }
}