using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Stopping;

/// <summary>Stops only when every child criterion currently requests a stop.</summary>
public sealed class AllStoppingCriterion : IStoppingCriterion
{
    private readonly IReadOnlyList<IStoppingCriterion> _criteria;

    public AllStoppingCriterion(params IStoppingCriterion[] criteria)
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

    public string Name => "All";

    public StoppingDecision Evaluate(in OptimizationState state, OptimizationSense sense)
    {
        foreach (IStoppingCriterion criterion in _criteria)
        {
            StoppingDecision decision = criterion.Evaluate(in state, sense);
            if (!decision.ShouldStop)
            {
                return StoppingDecision.Continue(Name);
            }
        }

        return StoppingDecision.Stop(Name, "All stopping criteria are satisfied.");
    }
}