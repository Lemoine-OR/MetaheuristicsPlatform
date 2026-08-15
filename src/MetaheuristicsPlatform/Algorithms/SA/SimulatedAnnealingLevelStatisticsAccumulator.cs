namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Allocation-free Welford accumulator used only when an adaptive statistical
/// cooling schedule requests level objective statistics.
/// </summary>
internal struct SimulatedAnnealingLevelStatisticsAccumulator
{
    private long _count;
    private double _mean;
    private double _m2;

    public long Count => _count;

    public double Mean =>
        _count == 0
            ? double.NaN
            : _mean;

    public double PopulationVariance =>
        _count < 2
            ? 0.0
            : _m2 / _count;

    public void Record(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new InvalidOperationException(
                "Statistical SA cooling requires finite objective values.");
        }

        _count++;

        double delta =
            value -
            _mean;

        _mean +=
            delta /
            _count;

        double delta2 =
            value -
            _mean;

        _m2 +=
            delta *
            delta2;
    }

    public void Reset()
    {
        _count = 0;
        _mean = 0.0;
        _m2 = 0.0;
    }
}
