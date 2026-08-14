using System.Runtime.CompilerServices;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Reusable flat parameter storage for adaptive DE variants.
/// Parent parameters represent inherited state; trial parameters represent the
/// values proposed for the current generation.
/// </summary>
public sealed class DeParameterBuffers
{
    private readonly double[] _parentF;
    private readonly double[] _parentCr;
    private readonly double[] _trialF;
    private readonly double[] _trialCr;

    public DeParameterBuffers(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity));
        }

        Capacity = capacity;

        _parentF =
            GC.AllocateUninitializedArray<double>(
                capacity);

        _parentCr =
            GC.AllocateUninitializedArray<double>(
                capacity);

        _trialF =
            GC.AllocateUninitializedArray<double>(
                capacity);

        _trialCr =
            GC.AllocateUninitializedArray<double>(
                capacity);
    }

    public int Capacity { get; }

    public Span<double> ParentDifferentialWeights =>
        _parentF;

    public Span<double> ParentCrossoverProbabilities =>
        _parentCr;

    public Span<double> TrialDifferentialWeights =>
        _trialF;

    public Span<double> TrialCrossoverProbabilities =>
        _trialCr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DeControlParameters GetParent(int target)
    {
        ValidateTarget(target);

        return new DeControlParameters(
            _parentF[target],
            _parentCr[target]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DeControlParameters GetTrial(int target)
    {
        ValidateTarget(target);

        return new DeControlParameters(
            _trialF[target],
            _trialCr[target]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetParent(
        int target,
        in DeControlParameters parameters)
    {
        ValidateTarget(target);

        _parentF[target] =
            parameters.DifferentialWeight;

        _parentCr[target] =
            parameters.CrossoverProbability;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetTrial(
        int target,
        in DeControlParameters parameters)
    {
        ValidateTarget(target);

        _trialF[target] =
            parameters.DifferentialWeight;

        _trialCr[target] =
            parameters.CrossoverProbability;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AcceptTrial(int target)
    {
        ValidateTarget(target);

        _parentF[target] =
            _trialF[target];

        _parentCr[target] =
            _trialCr[target];
    }

    private void ValidateTarget(int target)
    {
        if ((uint)target >=
            (uint)Capacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(target));
        }
    }
}