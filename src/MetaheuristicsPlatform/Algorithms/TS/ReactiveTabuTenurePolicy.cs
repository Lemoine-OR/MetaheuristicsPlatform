namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Reactive prohibition-period controller based on Battiti and Tecchiolli (1994).
/// </summary>
/// <remarks>
/// The original Reactive Tabu Search learns the prohibition period from repetitions:
/// it grows when cycles are detected, decreases when repetition evidence disappears,
/// and requests an escape phase when repetitions persist. The exact numerical reaction
/// constants are problem/configuration choices, so this implementation exposes them rather
/// than pretending that one universal set of constants was prescribed by the paper.
/// </remarks>
public sealed class ReactiveTabuTenurePolicy : IReactiveTabuTenurePolicy
{
    private readonly int _minimumTenure;
    private readonly int _maximumTenure;
    private readonly double _increaseFactor;
    private readonly double _decreaseFactor;
    private readonly int _decreaseAfterIterationsWithoutRepetition;
    private readonly double _cycleMovingAverageAlpha;
    private readonly int _diversificationRepetitionThreshold;
    private readonly double _diversificationCycleMultiplier;
    private readonly int _maximumDiversificationMoves;

    private long _lastRepetitionIteration = -1;
    private long _lastDecreaseIteration;
    private int _repetitionsSinceDiversification;

    public ReactiveTabuTenurePolicy(
        int initialTenure = 1,
        int minimumTenure = 1,
        int maximumTenure = 128,
        double increaseFactor = 1.3,
        double decreaseFactor = 0.9,
        int decreaseAfterIterationsWithoutRepetition = 100,
        double cycleMovingAverageAlpha = 0.1,
        int diversificationRepetitionThreshold = 3,
        double diversificationCycleMultiplier = 1.0,
        int maximumDiversificationMoves = 10_000)
    {
        if (minimumTenure <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumTenure));
        }

        if (maximumTenure < minimumTenure)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumTenure));
        }

        if (initialTenure < minimumTenure || initialTenure > maximumTenure)
        {
            throw new ArgumentOutOfRangeException(nameof(initialTenure));
        }

        if (!double.IsFinite(increaseFactor) || increaseFactor <= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(increaseFactor));
        }

        if (!double.IsFinite(decreaseFactor) ||
            decreaseFactor <= 0.0 ||
            decreaseFactor >= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(decreaseFactor));
        }

        if (decreaseAfterIterationsWithoutRepetition <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(decreaseAfterIterationsWithoutRepetition));
        }

        if (!double.IsFinite(cycleMovingAverageAlpha) ||
            cycleMovingAverageAlpha <= 0.0 ||
            cycleMovingAverageAlpha > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(cycleMovingAverageAlpha));
        }

        if (diversificationRepetitionThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(diversificationRepetitionThreshold));
        }

        if (!double.IsFinite(diversificationCycleMultiplier) ||
            diversificationCycleMultiplier <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(diversificationCycleMultiplier));
        }

        if (maximumDiversificationMoves <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumDiversificationMoves));
        }

        CurrentTenure = initialTenure;
        _minimumTenure = minimumTenure;
        _maximumTenure = maximumTenure;
        _increaseFactor = increaseFactor;
        _decreaseFactor = decreaseFactor;
        _decreaseAfterIterationsWithoutRepetition =
            decreaseAfterIterationsWithoutRepetition;
        _cycleMovingAverageAlpha = cycleMovingAverageAlpha;
        _diversificationRepetitionThreshold =
            diversificationRepetitionThreshold;
        _diversificationCycleMultiplier =
            diversificationCycleMultiplier;
        _maximumDiversificationMoves =
            maximumDiversificationMoves;
    }

    public int CurrentTenure { get; private set; }

    public double MovingAverageCycleLength { get; private set; }

    public long RepetitionsObserved { get; private set; }

    public ReactiveTabuReaction Observe(
        in ReactiveTabuTenureContext context)
    {
        bool tenureChanged = false;

        if (context.Repetition.IsRepetition)
        {
            RepetitionsObserved++;
            _repetitionsSinceDiversification++;
            _lastRepetitionIteration = context.Iteration;

            double cycleLength = context.Repetition.CycleLength;
            MovingAverageCycleLength =
                MovingAverageCycleLength <= 0.0
                    ? cycleLength
                    : (_cycleMovingAverageAlpha * cycleLength) +
                      ((1.0 - _cycleMovingAverageAlpha) *
                       MovingAverageCycleLength);

            double proposedIncrease =
                CurrentTenure * _increaseFactor;

            int increased =
                proposedIncrease >= _maximumTenure
                    ? _maximumTenure
                    : (int)Math.Ceiling(proposedIncrease);

            if (increased <= CurrentTenure &&
                CurrentTenure < _maximumTenure)
            {
                increased = CurrentTenure + 1;
            }

            if (increased != CurrentTenure)
            {
                CurrentTenure = increased;
                tenureChanged = true;
            }
        }
        else if (_lastRepetitionIteration >= 0 &&
                 context.Iteration - _lastRepetitionIteration >=
                     _decreaseAfterIterationsWithoutRepetition &&
                 context.Iteration - _lastDecreaseIteration >=
                     _decreaseAfterIterationsWithoutRepetition)
        {
            int decreased = (int)Math.Floor(
                CurrentTenure * _decreaseFactor);

            if (decreased >= CurrentTenure &&
                CurrentTenure > _minimumTenure)
            {
                decreased = CurrentTenure - 1;
            }

            decreased = Math.Max(_minimumTenure, decreased);
            _lastDecreaseIteration = context.Iteration;

            if (decreased != CurrentTenure)
            {
                CurrentTenure = decreased;
                tenureChanged = true;
            }
        }

        bool diversificationRequested =
            _repetitionsSinceDiversification >=
            _diversificationRepetitionThreshold;

        int diversificationMoves = 0;

        if (diversificationRequested)
        {
            double basis = Math.Max(
                1.0,
                MovingAverageCycleLength);

            double proposedMoves =
                basis * _diversificationCycleMultiplier;

            diversificationMoves =
                proposedMoves >= _maximumDiversificationMoves
                    ? _maximumDiversificationMoves
                    : Math.Max(
                        1,
                        (int)Math.Ceiling(proposedMoves));
        }

        return new ReactiveTabuReaction(
            CurrentTenure,
            tenureChanged,
            diversificationRequested,
            diversificationMoves);
    }

    public void AcknowledgeDiversification()
    {
        _repetitionsSinceDiversification = 0;
    }
}
