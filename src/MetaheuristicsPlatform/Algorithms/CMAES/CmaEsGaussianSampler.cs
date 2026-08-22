using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>
/// Deterministic standard-normal sampler using the polar-free Box-Muller
/// transform over the platform random source.
/// </summary>
internal sealed class CmaEsGaussianSampler
{
    private bool _hasSpare;
    private double _spare;

    public double Next(IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (_hasSpare)
        {
            _hasSpare = false;
            return _spare;
        }

        double u1 =
            Math.Max(
                double.Epsilon,
                random.NextDouble());

        double u2 =
            random.NextDouble();

        if (!double.IsFinite(u1) ||
            !double.IsFinite(u2) ||
            u1 <= 0.0 ||
            u1 > 1.0 ||
            u2 < 0.0 ||
            u2 >= 1.0)
        {
            throw new InvalidOperationException(
                "The random source returned an invalid uniform variate.");
        }

        double radius =
            Math.Sqrt(
                -2.0 * Math.Log(u1));

        double angle =
            2.0 * Math.PI * u2;

        double first =
            radius * Math.Cos(angle);

        _spare =
            radius * Math.Sin(angle);

        _hasSpare = true;
        return first;
    }

    public void Fill(
        IRandomSource random,
        Span<double> destination)
    {
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = Next(random);
        }
    }
}
