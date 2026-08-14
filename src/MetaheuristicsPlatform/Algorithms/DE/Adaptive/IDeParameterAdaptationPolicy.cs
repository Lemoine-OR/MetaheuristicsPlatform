using MetaheuristicsPlatform.Algorithms.DE.Random;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Bulk generation-level parameter adaptation contract.
///
/// The contract is intentionally coarse-grained: policies prepare all target
/// parameters and consume all selection feedback once per generation. This avoids
/// forcing an interface dispatch into every dimension of the DE hot loop.
/// </summary>
public interface IDeParameterAdaptationPolicy
{
    string Id { get; }

    DeParameterAdaptationKind Kind { get; }

    void Initialize(
        DeParameterBuffers buffers,
        int activePopulationSize);

    void PrepareGeneration(
        in DeGenerationAdaptationContext context,
        DeParameterBuffers buffers,
        DeTargetRandomStreams randomStreams);

    void CompleteGeneration(
        in DeGenerationAdaptationContext context,
        DeParameterBuffers buffers,
        ReadOnlySpan<DeSelectionFeedback> feedback);
}