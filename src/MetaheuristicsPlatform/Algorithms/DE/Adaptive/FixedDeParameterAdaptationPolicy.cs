using MetaheuristicsPlatform.Algorithms.DE.Random;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Fixed F/CR policy representing classical Differential Evolution.
/// </summary>
public sealed class FixedDeParameterAdaptationPolicy :
    IDeParameterAdaptationPolicy
{
    private readonly DeControlParameters _parameters;

    public FixedDeParameterAdaptationPolicy(
        double differentialWeight,
        double crossoverProbability)
    {
        _parameters =
            new DeControlParameters(
                differentialWeight,
                crossoverProbability);

        _parameters.Validate();
    }

    public string Id =>
        "fixed";

    public DeParameterAdaptationKind Kind =>
        DeParameterAdaptationKind.Fixed;

    public void Initialize(
        DeParameterBuffers buffers,
        int activePopulationSize)
    {
        ArgumentNullException.ThrowIfNull(buffers);

        ValidateActivePopulation(
            buffers,
            activePopulationSize);

        for (int target = 0;
             target < activePopulationSize;
             target++)
        {
            buffers.SetParent(
                target,
                in _parameters);

            buffers.SetTrial(
                target,
                in _parameters);
        }
    }

    public void PrepareGeneration(
        in DeGenerationAdaptationContext context,
        DeParameterBuffers buffers,
        DeTargetRandomStreams randomStreams)
    {
        ArgumentNullException.ThrowIfNull(buffers);
        ArgumentNullException.ThrowIfNull(randomStreams);

        ValidateActivePopulation(
            buffers,
            context.ActivePopulationSize);

        for (int target = 0;
             target < context.ActivePopulationSize;
             target++)
        {
            buffers.SetTrial(
                target,
                in _parameters);
        }
    }

    public void CompleteGeneration(
        in DeGenerationAdaptationContext context,
        DeParameterBuffers buffers,
        ReadOnlySpan<DeSelectionFeedback> feedback)
    {
        ArgumentNullException.ThrowIfNull(buffers);

        ValidateActivePopulation(
            buffers,
            context.ActivePopulationSize);

        if (feedback.Length !=
            context.ActivePopulationSize)
        {
            throw new ArgumentException(
                "Feedback length must match the active population size.",
                nameof(feedback));
        }

        // Classical fixed DE has no adaptive state to update.
    }

    private static void ValidateActivePopulation(
        DeParameterBuffers buffers,
        int activePopulationSize)
    {
        if (activePopulationSize <= 0 ||
            activePopulationSize > buffers.Capacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(activePopulationSize));
        }
    }
}