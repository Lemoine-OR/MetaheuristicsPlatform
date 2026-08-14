using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.SearchSpaces.Continuous;

/// <summary>
/// Defines a finite-dimensional continuous box-constrained search space.
/// </summary>
public interface IBoundedContinuousSearchSpace
{
    /// <summary>Gets the number of decision variables.</summary>
    int Dimension { get; }

    /// <summary>Gets lower bounds in variable order.</summary>
    ReadOnlySpan<double> LowerBounds { get; }

    /// <summary>Gets upper bounds in variable order.</summary>
    ReadOnlySpan<double> UpperBounds { get; }

    /// <summary>Samples one uniformly distributed point inside the box.</summary>
    void Sample(IRandomSource random, Span<double> destination);

    /// <summary>Returns whether a position belongs to the bounded search space.</summary>
    bool Contains(ReadOnlySpan<double> position);

    /// <summary>Clamps a position component-wise to the box bounds.</summary>
    void Clamp(Span<double> position);
}