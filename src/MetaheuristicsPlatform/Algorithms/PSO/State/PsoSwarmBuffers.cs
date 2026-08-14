using System.Runtime.CompilerServices;

namespace MetaheuristicsPlatform.Algorithms.PSO.State;

/// <summary>
/// Flat, particle-major PSO state buffers.
/// Allocation occurs once when the swarm is created; particle views are zero-allocation spans.
/// </summary>
public sealed class PsoSwarmBuffers
{
    private readonly double[] _positions;
    private readonly double[] _velocities;
    private readonly double[] _personalBestPositions;
    private readonly double[] _currentFitness;
    private readonly double[] _personalBestFitness;

    public PsoSwarmBuffers(
        int swarmSize,
        int dimension)
    {
        if (swarmSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(swarmSize));
        }

        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dimension));
        }

        int vectorElementCount =
            checked(swarmSize * dimension);

        SwarmSize = swarmSize;
        Dimension = dimension;

        _positions =
            GC.AllocateUninitializedArray<double>(
                vectorElementCount);

        _velocities =
            GC.AllocateUninitializedArray<double>(
                vectorElementCount);

        _personalBestPositions =
            GC.AllocateUninitializedArray<double>(
                vectorElementCount);

        _currentFitness =
            GC.AllocateUninitializedArray<double>(
                swarmSize);

        _personalBestFitness =
            GC.AllocateUninitializedArray<double>(
                swarmSize);

        Array.Fill(
            _currentFitness,
            double.NaN);

        Array.Fill(
            _personalBestFitness,
            double.NaN);
    }

    public int SwarmSize { get; }
    public int Dimension { get; }

    /// <summary>
    /// Borrowed flat position buffer. Particle i starts at i * Dimension.
    /// </summary>
    public double[] Positions => _positions;

    /// <summary>Borrowed flat velocity buffer.</summary>
    public double[] Velocities => _velocities;

    /// <summary>Borrowed flat personal-best-position buffer.</summary>
    public double[] PersonalBestPositions =>
        _personalBestPositions;

    /// <summary>Borrowed current-fitness buffer.</summary>
    public double[] CurrentFitness =>
        _currentFitness;

    /// <summary>Borrowed personal-best-fitness buffer.</summary>
    public double[] PersonalBestFitness =>
        _personalBestFitness;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<double> GetPosition(int particle) =>
        _positions.AsSpan(
            Offset(particle),
            Dimension);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<double> GetVelocity(int particle) =>
        _velocities.AsSpan(
            Offset(particle),
            Dimension);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<double> GetPersonalBestPosition(
        int particle) =>
        _personalBestPositions.AsSpan(
            Offset(particle),
            Dimension);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<double> GetPositionReadOnly(
        int particle) =>
        _positions.AsSpan(
            Offset(particle),
            Dimension);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<double> GetVelocityReadOnly(
        int particle) =>
        _velocities.AsSpan(
            Offset(particle),
            Dimension);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<double>
        GetPersonalBestPositionReadOnly(
            int particle) =>
        _personalBestPositions.AsSpan(
            Offset(particle),
            Dimension);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Offset(int particle)
    {
        if ((uint)particle >= (uint)SwarmSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(particle));
        }

        return particle * Dimension;
    }
}