namespace MetaheuristicsPlatform.Algorithms.TA;

/// <summary>Built-in monotone Threshold Accepting threshold schedules.</summary>
public enum ThresholdAcceptingScheduleKind
{
    Linear = 0,
    Geometric = 1,
    Explicit = 2
}

/// <summary>Context supplied when a threshold level is completed.</summary>
public readonly record struct ThresholdAcceptingScheduleContext(
    long CompletedThresholdLevels,
    long AttemptedTransitions,
    long AcceptedTransitions,
    double InitialThreshold,
    double CurrentThreshold);

/// <summary>Produces the threshold for the next trajectory level.</summary>
public interface IThresholdAcceptingSchedule
{
    double GetNextThreshold(
        in ThresholdAcceptingScheduleContext context);
}

/// <summary>
/// Linear monotone threshold reduction.
/// </summary>
public sealed class LinearThresholdSchedule :
    IThresholdAcceptingSchedule
{
    public LinearThresholdSchedule(
        double decrement)
    {
        if (!double.IsFinite(decrement) ||
            decrement <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(decrement));
        }

        Decrement =
            decrement;
    }

    public double Decrement { get; }

    public double GetNextThreshold(
        in ThresholdAcceptingScheduleContext context) =>
        Math.Max(
            0.0,
            context.CurrentThreshold -
            Decrement);
}

/// <summary>
/// Geometric monotone threshold reduction.
/// </summary>
public sealed class GeometricThresholdSchedule :
    IThresholdAcceptingSchedule
{
    public GeometricThresholdSchedule(
        double alpha)
    {
        if (!double.IsFinite(alpha) ||
            alpha <= 0.0 ||
            alpha >= 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(alpha));
        }

        Alpha =
            alpha;
    }

    public double Alpha { get; }

    public double GetNextThreshold(
        in ThresholdAcceptingScheduleContext context) =>
        context.CurrentThreshold *
        Alpha;
}

/// <summary>
/// User-supplied finite non-increasing threshold sequence.
/// </summary>
/// <remarks>
/// The values represent thresholds used after successive completed levels. Once the
/// sequence is exhausted, the final value is retained. This is the direct generic
/// representation of the explicit threshold-list formulation used in classical TA.
/// </remarks>
public sealed class ExplicitThresholdSchedule :
    IThresholdAcceptingSchedule
{
    private readonly double[] _thresholds;

    public ExplicitThresholdSchedule(
        IEnumerable<double> thresholds)
    {
        ArgumentNullException.ThrowIfNull(thresholds);

        _thresholds =
            thresholds.ToArray();

        if (_thresholds.Length == 0)
        {
            throw new ArgumentException(
                "At least one explicit threshold is required.",
                nameof(thresholds));
        }

        double previous =
            double.PositiveInfinity;

        for (int i = 0;
             i < _thresholds.Length;
             i++)
        {
            double threshold =
                _thresholds[i];

            if (!double.IsFinite(threshold) ||
                threshold < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(thresholds),
                    "Explicit thresholds must be finite and non-negative.");
            }

            if (threshold > previous)
            {
                throw new ArgumentException(
                    "Explicit thresholds must be non-increasing.",
                    nameof(thresholds));
            }

            previous =
                threshold;
        }
    }

    public double GetNextThreshold(
        in ThresholdAcceptingScheduleContext context)
    {
        long zeroBased =
            Math.Max(
                0L,
                context.CompletedThresholdLevels - 1L);

        int index =
            (int)Math.Min(
                zeroBased,
                _thresholds.Length - 1L);

        return
            _thresholds[index];
    }
}