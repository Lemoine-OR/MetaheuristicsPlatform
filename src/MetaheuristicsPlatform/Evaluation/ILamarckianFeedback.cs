namespace MetaheuristicsPlatform.Evaluation;

/// <summary>
/// Projects an improved phenotype back into the metaheuristic candidate/encoding.
/// </summary>
public interface ILamarckianFeedback<TCandidate, in TSolution>
{
    void Apply(
        TSolution improvedSolution,
        ref TCandidate candidate,
        CancellationToken cancellationToken = default);
}