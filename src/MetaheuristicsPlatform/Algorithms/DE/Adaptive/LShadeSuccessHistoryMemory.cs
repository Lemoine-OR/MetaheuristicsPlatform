namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// SHADE 1.1 historical memory used by L-SHADE.
/// </summary>
/// <remarks>
/// M_CR supports the terminal value described by Tanabe and Fukunaga.
/// A terminal CR memory slot always generates CR = 0 and remains terminal.
///
/// Reference:
/// R. Tanabe, A. S. Fukunaga,
/// IEEE CEC 2014, 1658-1665.
/// DOI: 10.1109/CEC.2014.6900380.
/// </remarks>
public sealed class LShadeSuccessHistoryMemory
{
    private readonly double[] _differentialWeights;
    private readonly double[] _crossoverProbabilities;
    private readonly bool[] _terminalCrossover;

    public LShadeSuccessHistoryMemory(
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

        _terminalCrossover =
            new bool[capacity];

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

    public double GetDifferentialWeight(
        int index)
    {
        ValidateIndex(index);

        return _differentialWeights[index];
    }

    public bool IsCrossoverTerminal(
        int index)
    {
        ValidateIndex(index);

        return _terminalCrossover[index];
    }

    public double GetCrossoverProbability(
        int index)
    {
        ValidateIndex(index);

        return _terminalCrossover[index]
            ? 0.0
            : _crossoverProbabilities[index];
    }

    public void UpdateTerminalCrossover()
    {
        _terminalCrossover[Position] =
            true;

        Advance();
    }

    public void Update(
        double weightedLehmerMeanCr,
        double weightedLehmerMeanF)
    {
        if (_terminalCrossover[Position])
        {
            Advance();
            return;
        }

        if (!double.IsFinite(weightedLehmerMeanCr) ||
            weightedLehmerMeanCr <= 0.0 ||
            weightedLehmerMeanCr > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(weightedLehmerMeanCr));
        }

        if (!double.IsFinite(weightedLehmerMeanF) ||
            weightedLehmerMeanF <= 0.0 ||
            weightedLehmerMeanF > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(weightedLehmerMeanF));
        }

        _crossoverProbabilities[Position] =
            weightedLehmerMeanCr;

        _differentialWeights[Position] =
            weightedLehmerMeanF;

        Advance();
    }

    private void Advance()
    {
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