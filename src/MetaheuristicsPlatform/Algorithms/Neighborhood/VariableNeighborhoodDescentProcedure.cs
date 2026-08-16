using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>
/// Reusable Variable Neighborhood Descent (VND) procedure.
/// The ordered local-search neighborhoods are explored sequentially. A strict improvement
/// restarts the sequence at the first neighborhood; otherwise the next neighborhood is tried.
/// </summary>
public sealed class VariableNeighborhoodDescentProcedure<TSolution> :
    ILocalSearchProcedure<TSolution>
{
    private readonly ILocalSearchProcedure<TSolution>[] _neighborhoods;
    private readonly int _maximumNeighborhoodRestarts;

    /// <summary>Creates an ordered VND procedure.</summary>
    public VariableNeighborhoodDescentProcedure(
        IReadOnlyList<ILocalSearchProcedure<TSolution>> neighborhoods,
        int maximumNeighborhoodRestarts = 10_000)
    {
        ArgumentNullException.ThrowIfNull(neighborhoods);

        if (neighborhoods.Count == 0)
        {
            throw new ArgumentException(
                "At least one local-search neighborhood is required.",
                nameof(neighborhoods));
        }

        if (maximumNeighborhoodRestarts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumNeighborhoodRestarts));
        }

        _neighborhoods = new ILocalSearchProcedure<TSolution>[neighborhoods.Count];
        for (int i = 0; i < neighborhoods.Count; i++)
        {
            _neighborhoods[i] = neighborhoods[i] ??
                throw new ArgumentException(
                    "Local-search neighborhoods must not contain null entries.",
                    nameof(neighborhoods));
        }

        _maximumNeighborhoodRestarts = maximumNeighborhoodRestarts;
    }

    /// <summary>Number of ordered local-search neighborhoods.</summary>
    public int NeighborhoodCount => _neighborhoods.Length;

    /// <inheritdoc />
    public LocalSearchProcedureResult Improve(
        ref TSolution solution,
        double currentFitness,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(solutionCloner);

        int neighborhoodIndex = 0;
        int restarts = 0;
        long acceptedMoves = 0;

        while (neighborhoodIndex < _neighborhoods.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double beforeFitness = currentFitness;

            LocalSearchProcedureResult result =
                _neighborhoods[neighborhoodIndex].Improve(
                    ref solution,
                    currentFitness,
                    context,
                    solutionCloner,
                    cancellationToken);

            currentFitness = result.Fitness;
            acceptedMoves += result.AcceptedMoves;

            if (result.StoppingDecision.ShouldStop)
            {
                return new LocalSearchProcedureResult(
                    currentFitness,
                    acceptedMoves,
                    localOptimum: false,
                    result.StoppingDecision);
            }

            if (context.Problem.Sense.IsBetter(currentFitness, beforeFitness))
            {
                restarts++;

                if (restarts >= _maximumNeighborhoodRestarts)
                {
                    return new LocalSearchProcedureResult(
                        currentFitness,
                        acceptedMoves,
                        localOptimum: false,
                        StoppingDecision.Continue("MaximumNeighborhoodRestarts"));
                }

                neighborhoodIndex = 0;
            }
            else
            {
                neighborhoodIndex++;
            }

            var state = new VariableNeighborhoodDescentState(
                NeighborhoodIndex:
                    Math.Min(neighborhoodIndex + 1, _neighborhoods.Length),
                NeighborhoodCount: _neighborhoods.Length,
                NeighborhoodRestarts: restarts,
                AcceptedLocalMoves: acceptedMoves);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
            {
                return new LocalSearchProcedureResult(
                    currentFitness,
                    acceptedMoves,
                    localOptimum: false,
                    stop);
            }
        }

        return new LocalSearchProcedureResult(
            currentFitness,
            acceptedMoves,
            localOptimum: true,
            StoppingDecision.Continue("VariableNeighborhoodLocalOptimum"));
    }
}

/// <summary>Observable state exposed while Variable Neighborhood Descent is running.</summary>
public readonly record struct VariableNeighborhoodDescentState(
    int NeighborhoodIndex,
    int NeighborhoodCount,
    int NeighborhoodRestarts,
    long AcceptedLocalMoves);
