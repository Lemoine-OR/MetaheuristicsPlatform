using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.TA;

/// <summary>Parameters for Dueck-Scheuer Threshold Accepting.</summary>
public sealed class ThresholdAcceptingParameters :
    IMetaheuristicParameters
{
    /// <summary>Initial maximum accepted objective degradation.</summary>
    public double InitialThreshold { get; init; } =
        1.0;

    /// <summary>Smallest threshold enforced by the optimizer.</summary>
    public double MinimumThreshold { get; init; } =
        0.0;

    /// <summary>Attempted transitions before advancing one threshold level.</summary>
    public int TransitionsPerThresholdLevel { get; init; } =
        100;

    /// <summary>Built-in threshold schedule.</summary>
    public ThresholdAcceptingScheduleKind ThresholdSchedule { get; init; } =
        ThresholdAcceptingScheduleKind.Linear;

    /// <summary>
    /// Optional user schedule. When supplied it overrides the built-in schedule kind.
    /// </summary>
    public IThresholdAcceptingSchedule? CustomThresholdSchedule { get; init; }

    /// <summary>Linear decrement used by the default schedule.</summary>
    public double LinearDecrement { get; init; } =
        0.01;

    /// <summary>Geometric contraction factor in (0,1).</summary>
    public double GeometricAlpha { get; init; } =
        0.95;

    /// <summary>
    /// Explicit non-increasing thresholds used when ThresholdSchedule is Explicit.
    /// These values are the thresholds after successive completed levels.
    /// </summary>
    public IReadOnlyList<double>? ExplicitThresholds { get; init; }

    /// <summary>Stops the method once MinimumThreshold has been reached.</summary>
    public bool StopAtMinimumThreshold { get; init; } =
        true;

    /// <summary>Maximum consecutive failures of the stochastic neighborhood sampler.</summary>
    public int MaximumConsecutiveSamplingFailures { get; init; } =
        64;

    public void Validate()
    {
        if (!double.IsFinite(InitialThreshold) ||
            InitialThreshold < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(InitialThreshold));
        }

        if (!double.IsFinite(MinimumThreshold) ||
            MinimumThreshold < 0.0 ||
            MinimumThreshold > InitialThreshold)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumThreshold));
        }

        if (TransitionsPerThresholdLevel <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(TransitionsPerThresholdLevel));
        }

        if (MaximumConsecutiveSamplingFailures <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumConsecutiveSamplingFailures));
        }

        IThresholdAcceptingSchedule schedule =
            CreateThresholdSchedule();

        if (schedule is ExplicitThresholdSchedule &&
            ExplicitThresholds is not null &&
            ExplicitThresholds.Count > 0 &&
            ExplicitThresholds[0] > InitialThreshold)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ExplicitThresholds),
                "The first explicit next threshold cannot exceed InitialThreshold.");
        }
    }

    internal IThresholdAcceptingSchedule
        CreateThresholdSchedule()
    {
        if (CustomThresholdSchedule is not null)
        {
            return
                CustomThresholdSchedule;
        }

        return
            ThresholdSchedule switch
            {
                ThresholdAcceptingScheduleKind.Linear =>
                    new LinearThresholdSchedule(
                        LinearDecrement),

                ThresholdAcceptingScheduleKind.Geometric =>
                    new GeometricThresholdSchedule(
                        GeometricAlpha),

                ThresholdAcceptingScheduleKind.Explicit =>
                    new ExplicitThresholdSchedule(
                        ExplicitThresholds ??
                        throw new InvalidOperationException(
                            "ExplicitThresholds must be supplied for the Explicit schedule.")),

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(ThresholdSchedule))
            };
    }
}