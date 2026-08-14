using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Standard result returned by a metaheuristic run.
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
public sealed record OptimizationResult<TSolution>
{
    public required MetaheuristicDescriptor Algorithm { get; init; }
    public required TSolution BestSolution { get; init; }
    public required double BestFitness { get; init; }
    public required OptimizationRunStatistics Statistics { get; init; }
    public required StoppingDecision StopDecision { get; init; }
    public required ulong Seed { get; init; }
    public required string RandomSourceId { get; init; }
}