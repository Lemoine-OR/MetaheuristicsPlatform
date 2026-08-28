using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.VectorNichePso;

public sealed class VectorNichePsoParameters : IMetaheuristicParameters
{
    public int SwarmSize { get; init; } = 80;
    public int MaximumIterations { get; init; } = 180;
    public double Inertia { get; init; } = 0.7;
    public double Cognitive { get; init; } = 1.5;
    public double Social { get; init; } = 1.5;
    public double NicheRadius { get; init; } = 0.1;
    public int MaximumOptima { get; init; } = 20;

    public void Validate()
    {
        if (SwarmSize < 4)
            throw new ArgumentOutOfRangeException(nameof(SwarmSize));
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(Inertia) || Inertia < 0.0)
            throw new ArgumentOutOfRangeException(nameof(Inertia));
        if (!double.IsFinite(Cognitive) || Cognitive < 0.0)
            throw new ArgumentOutOfRangeException(nameof(Cognitive));
        if (!double.IsFinite(Social) || Social < 0.0)
            throw new ArgumentOutOfRangeException(nameof(Social));
        if (!double.IsFinite(NicheRadius) || NicheRadius <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(NicheRadius));
        if (MaximumOptima <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumOptima));

    }
}
