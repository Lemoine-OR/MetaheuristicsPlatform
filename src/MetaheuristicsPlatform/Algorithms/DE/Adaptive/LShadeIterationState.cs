namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

public readonly record struct LShadeIterationState(
    int InitialPopulationSize,
    int ActivePopulationSize,
    int MinimumPopulationSize,
    int Dimension,
    int SuccessfulTrials,
    int ArchiveCount,
    int ArchiveLimit,
    int MemoryPosition,
    long FunctionEvaluations,
    long MaximumFunctionEvaluations)
{
    public double EvaluationProgress =>
        MaximumFunctionEvaluations <= 0
            ? 0.0
            : Math.Clamp(
                (double)FunctionEvaluations /
                MaximumFunctionEvaluations,
                0.0,
                1.0);
}