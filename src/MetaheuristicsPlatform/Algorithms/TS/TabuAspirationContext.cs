namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Candidate information supplied to a tabu aspiration criterion.
/// </summary>
public readonly struct TabuAspirationContext
{
    public TabuAspirationContext(
        long iteration,
        long evaluationIndex,
        double currentObjective,
        double bestObjective,
        double candidateObjective)
    {
        Iteration = iteration;
        EvaluationIndex = evaluationIndex;
        CurrentObjective = currentObjective;
        BestObjective = bestObjective;
        CandidateObjective = candidateObjective;
    }

    public long Iteration { get; }
    public long EvaluationIndex { get; }
    public double CurrentObjective { get; }
    public double BestObjective { get; }
    public double CandidateObjective { get; }
}
