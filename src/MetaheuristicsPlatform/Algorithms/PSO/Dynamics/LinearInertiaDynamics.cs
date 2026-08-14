namespace MetaheuristicsPlatform.Algorithms.PSO.Dynamics;

/// <summary>
/// Linearly interpolated inertia schedule over a configured iteration horizon.
/// </summary>
public sealed class LinearInertiaDynamics :
    IPsoVelocityDynamics
{
    public LinearInertiaDynamics(
        double initialWeight,
        double finalWeight,
        long transitionIterations)
    {
        if (!double.IsFinite(initialWeight) ||
            initialWeight < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialWeight));
        }

        if (!double.IsFinite(finalWeight) ||
            finalWeight < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(finalWeight));
        }

        if (transitionIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(transitionIterations));
        }

        InitialWeight = initialWeight;
        FinalWeight = finalWeight;
        TransitionIterations =
            transitionIterations;
    }

    public double InitialWeight { get; }
    public double FinalWeight { get; }
    public long TransitionIterations { get; }

    public string Id => "linear-inertia";

    public PsoVelocityCoefficients GetCoefficients(
        long iteration)
    {
        double progress =
            Math.Clamp(
                iteration /
                    (double)TransitionIterations,
                0.0,
                1.0);

        double weight =
            InitialWeight +
            ((FinalWeight - InitialWeight) *
                progress);

        return new(
            weight,
            1.0);
    }
}