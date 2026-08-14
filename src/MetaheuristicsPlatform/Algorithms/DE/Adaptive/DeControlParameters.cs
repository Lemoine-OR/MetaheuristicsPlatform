namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Differential Evolution control parameters for one target/trial.
/// </summary>
public readonly record struct DeControlParameters(
    double DifferentialWeight,
    double CrossoverProbability)
{
    public void Validate()
    {
        if (!double.IsFinite(DifferentialWeight) ||
            DifferentialWeight <= 0.0 ||
            DifferentialWeight > 2.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(DifferentialWeight));
        }

        if (!double.IsFinite(CrossoverProbability) ||
            CrossoverProbability < 0.0 ||
            CrossoverProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(CrossoverProbability));
        }
    }
}