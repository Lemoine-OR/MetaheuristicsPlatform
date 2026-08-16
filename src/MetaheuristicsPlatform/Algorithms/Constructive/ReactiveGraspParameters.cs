using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>Parameters for Reactive GRASP following Prais and Ribeiro.</summary>
public sealed class ReactiveGraspParameters : IMetaheuristicParameters
{
    private double[] _alphaValues =
    [
        0.0, 0.1, 0.2, 0.3, 0.4,
        0.5, 0.6, 0.7, 0.8, 0.9, 1.0
    ];

    /// <summary>Maximum number of complete construction + local-search iterations.</summary>
    public int MaximumIterations { get; set; } = 200;

    /// <summary>
    /// Discrete alpha set sampled by the reactive controller.
    /// The default platform grid is 0.0, 0.1, ..., 1.0.
    /// </summary>
    public double[] AlphaValues
    {
        get => _alphaValues;
        set => _alphaValues =
            value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Number of completed Reactive GRASP observations between probability-update attempts.
    /// An update is performed only after every configured alpha has been observed.
    /// </summary>
    public int ProbabilityUpdatePeriod { get; set; } = 10;

    /// <summary>Safety bound for construction components accepted in one GRASP construction.</summary>
    public int MaximumConstructionSteps { get; set; } = int.MaxValue;

    /// <inheritdoc />
    public void Validate()
    {
        if (MaximumIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        }

        if (_alphaValues.Length == 0)
        {
            throw new ArgumentException(
                "Reactive GRASP requires at least one alpha value.",
                nameof(AlphaValues));
        }

        for (int i = 0; i < _alphaValues.Length; i++)
        {
            double alpha = _alphaValues[i];

            if (!double.IsFinite(alpha) || alpha < 0.0 || alpha > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(AlphaValues),
                    "Every Reactive GRASP alpha value must be finite and lie in [0,1].");
            }

            for (int j = 0; j < i; j++)
            {
                if (_alphaValues[j].Equals(alpha))
                {
                    throw new ArgumentException(
                        "Reactive GRASP alpha values must be unique.",
                        nameof(AlphaValues));
                }
            }
        }

        if (ProbabilityUpdatePeriod <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ProbabilityUpdatePeriod));
        }

        if (MaximumConstructionSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumConstructionSteps));
        }
    }
}
