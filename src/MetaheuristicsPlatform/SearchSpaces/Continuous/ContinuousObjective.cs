namespace MetaheuristicsPlatform.SearchSpaces.Continuous;

/// <summary>
/// Allocation-free objective delegate for continuous vector optimization.
/// </summary>
/// <param name="position">Read-only decision vector.</param>
/// <returns>Objective value.</returns>
public delegate double ContinuousObjective(ReadOnlySpan<double> position);