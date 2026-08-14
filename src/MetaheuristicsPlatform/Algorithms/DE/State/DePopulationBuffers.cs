using System.Runtime.CompilerServices;

namespace MetaheuristicsPlatform.Algorithms.DE.State;

/// <summary>
/// Flat target-major DE buffers.
/// </summary>
public sealed class DePopulationBuffers
{
    private readonly double[] _population;
    private readonly double[] _trialPopulation;
    private readonly double[] _fitness;
    private readonly double[] _trialFitness;

    public DePopulationBuffers(
        int populationSize,
        int dimension)
    {
        if (populationSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(populationSize));
        }

        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        PopulationSize = populationSize;
        Dimension = dimension;

        int vectorCount =
            checked(
                populationSize *
                dimension);

        _population =
            GC.AllocateUninitializedArray<double>(
                vectorCount);

        _trialPopulation =
            GC.AllocateUninitializedArray<double>(
                vectorCount);

        _fitness =
            GC.AllocateUninitializedArray<double>(
                populationSize);

        _trialFitness =
            GC.AllocateUninitializedArray<double>(
                populationSize);
    }

    public int PopulationSize { get; }

    public int Dimension { get; }

    public Span<double> Fitness =>
        _fitness;

    public Span<double> TrialFitness =>
        _trialFitness;

    public ReadOnlySpan<double> FitnessReadOnly =>
        _fitness;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetFitness(
        int target)
    {
        ValidateTarget(target);
        return _fitness[target];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFitness(
        int target,
        double value)
    {
        ValidateTarget(target);
        _fitness[target] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetTrialFitness(
        int target)
    {
        ValidateTarget(target);
        return _trialFitness[target];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetTrialFitness(
        int target,
        double value)
    {
        ValidateTarget(target);
        _trialFitness[target] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<double> GetVector(int target)
    {
        ValidateTarget(target);

        return _population.AsSpan(
            target * Dimension,
            Dimension);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<double> GetVectorReadOnly(int target)
    {
        ValidateTarget(target);

        return _population.AsSpan(
            target * Dimension,
            Dimension);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<double> GetTrialVector(int target)
    {
        ValidateTarget(target);

        return _trialPopulation.AsSpan(
            target * Dimension,
            Dimension);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<double> GetTrialVectorReadOnly(int target)
    {
        ValidateTarget(target);

        return _trialPopulation.AsSpan(
            target * Dimension,
            Dimension);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateTarget(int target)
    {
        if ((uint)target >=
            (uint)PopulationSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(target));
        }
    }
}