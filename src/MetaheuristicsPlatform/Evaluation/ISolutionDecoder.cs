namespace MetaheuristicsPlatform.Evaluation;

/// <summary>Decodes a metaheuristic candidate/encoding into a problem solution.</summary>
public interface ISolutionDecoder<in TCandidate, out TSolution>
{
    TSolution Decode(
        TCandidate candidate,
        CancellationToken cancellationToken = default);
}