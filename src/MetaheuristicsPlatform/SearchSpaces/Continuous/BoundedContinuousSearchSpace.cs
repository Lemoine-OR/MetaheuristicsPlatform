using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.SearchSpaces.Continuous;

/// <summary>
/// Immutable box-constrained continuous search space.
/// </summary>
public sealed class BoundedContinuousSearchSpace : IBoundedContinuousSearchSpace
{
    private readonly double[] _lowerBounds;
    private readonly double[] _upperBounds;

    /// <summary>
    /// Initializes a bounded continuous search space from component-wise bounds.
    /// Defensive copies are created.
    /// </summary>
    public BoundedContinuousSearchSpace(
        IReadOnlyList<double> lowerBounds,
        IReadOnlyList<double> upperBounds)
    {
        ArgumentNullException.ThrowIfNull(lowerBounds);
        ArgumentNullException.ThrowIfNull(upperBounds);

        if (lowerBounds.Count == 0)
        {
            throw new ArgumentException("At least one dimension is required.", nameof(lowerBounds));
        }

        if (lowerBounds.Count != upperBounds.Count)
        {
            throw new ArgumentException("Lower and upper bounds must have the same dimension.");
        }

        _lowerBounds = new double[lowerBounds.Count];
        _upperBounds = new double[upperBounds.Count];

        for (int i = 0; i < lowerBounds.Count; i++)
        {
            double lower = lowerBounds[i];
            double upper = upperBounds[i];

            if (!double.IsFinite(lower))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lowerBounds),
                    $"Lower bound at dimension {i} must be finite.");
            }

            if (!double.IsFinite(upper))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(upperBounds),
                    $"Upper bound at dimension {i} must be finite.");
            }

            if (lower >= upper)
            {
                throw new ArgumentException(
                    $"Lower bound must be strictly smaller than upper bound at dimension {i}.");
            }

            double width = upper - lower;
            if (!double.IsFinite(width))
            {
                throw new ArgumentException(
                    $"The interval width at dimension {i} must be finite.");
            }

            _lowerBounds[i] = lower;
            _upperBounds[i] = upper;
        }
    }

    /// <summary>Creates a search space using the same bounds in every dimension.</summary>
    public static BoundedContinuousSearchSpace Uniform(
        int dimension,
        double lowerBound,
        double upperBound)
    {
        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dimension));
        }

        double[] lower = new double[dimension];
        double[] upper = new double[dimension];
        Array.Fill(lower, lowerBound);
        Array.Fill(upper, upperBound);

        return new BoundedContinuousSearchSpace(lower, upper);
    }

    /// <inheritdoc />
    public int Dimension => _lowerBounds.Length;

    /// <inheritdoc />
    public ReadOnlySpan<double> LowerBounds => _lowerBounds;

    /// <inheritdoc />
    public ReadOnlySpan<double> UpperBounds => _upperBounds;

    /// <inheritdoc />
    public void Sample(IRandomSource random, Span<double> destination)
    {
        ArgumentNullException.ThrowIfNull(random);
        ValidateDimension(destination.Length, nameof(destination));

        for (int i = 0; i < _lowerBounds.Length; i++)
        {
            double lower = _lowerBounds[i];
            double upper = _upperBounds[i];
            destination[i] = lower + ((upper - lower) * random.NextDouble());
        }
    }

    /// <inheritdoc />
    public bool Contains(ReadOnlySpan<double> position)
    {
        if (position.Length != _lowerBounds.Length)
        {
            return false;
        }

        for (int i = 0; i < _lowerBounds.Length; i++)
        {
            double value = position[i];
            if (double.IsNaN(value) ||
                value < _lowerBounds[i] ||
                value > _upperBounds[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public void Clamp(Span<double> position)
    {
        ValidateDimension(position.Length, nameof(position));

        for (int i = 0; i < _lowerBounds.Length; i++)
        {
            double value = position[i];

            if (double.IsNaN(value))
            {
                throw new ArgumentException(
                    $"Position contains NaN at dimension {i}.",
                    nameof(position));
            }

            position[i] = Math.Clamp(value, _lowerBounds[i], _upperBounds[i]);
        }
    }

    private void ValidateDimension(int dimension, string parameterName)
    {
        if (dimension != _lowerBounds.Length)
        {
            throw new ArgumentException(
                $"Expected dimension {_lowerBounds.Length}, received {dimension}.",
                parameterName);
        }
    }
}