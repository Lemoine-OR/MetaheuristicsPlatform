namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Circular success-history memory for SHADE control parameters.
/// </summary>
/// <remarks>
/// Reference:
/// R. Tanabe, A. Fukunaga,
/// "Success-History Based Parameter Adaptation for Differential Evolution",
/// IEEE CEC 2013, 71-78.
/// DOI: 10.1109/CEC.2013.6557555.
/// </remarks>
public sealed class ShadeSuccessHistoryMemory
{
    private readonly double[] _differentialWeights;
    private readonly double[] _crossoverProbabilities;

    public ShadeSuccessHistoryMemory(
        int capacity,
        double initialValue = 0.5)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity));
        }

        if (!double.IsFinite(initialValue) ||
            initialValue <= 0.0 ||
            initialValue > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialValue));
        }

        Capacity = capacity;

        _differentialWeights =
            GC.AllocateUninitializedArray<double>(
                capacity);

        _crossoverProbabilities =
            GC.AllocateUninitializedArray<double>(
                capacity);

        Array.Fill(
            _differentialWeights,
            initialValue);

        Array.Fill(
            _crossoverProbabilities,
            initialValue);
    }

    public int Capacity { get; }

    public int Position { get; private set; }

    public ReadOnlySpan<double> DifferentialWeights =>
        _differentialWeights;

    public ReadOnlySpan<double> CrossoverProbabilities =>
        _crossoverProbabilities;

    public double GetDifferentialWeight(
        int index)
    {
        ValidateIndex(index);

        return _differentialWeights[index];
    }

    public double GetCrossoverProbability(
        int index)
    {
        ValidateIndex(index);

        return _crossoverProbabilities[index];
    }

    public void Update(
        double weightedArithmeticMeanCr,
        double weightedLehmerMeanF)
    {
        if (!double.IsFinite(weightedArithmeticMeanCr) ||
            weightedArithmeticMeanCr < 0.0 ||
            weightedArithmeticMeanCr > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(weightedArithmeticMeanCr));
        }

        if (!double.IsFinite(weightedLehmerMeanF) ||
            weightedLehmerMeanF <= 0.0 ||
            weightedLehmerMeanF > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(weightedLehmerMeanF));
        }

        _crossoverProbabilities[Position] =
            weightedArithmeticMeanCr;

        _differentialWeights[Position] =
            weightedLehmerMeanF;

        Position++;

        if (Position == Capacity)
        {
            Position = 0;
        }
    }

    private void ValidateIndex(int index)
    {
        if ((uint)index >=
            (uint)Capacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index));
        }
    }
}