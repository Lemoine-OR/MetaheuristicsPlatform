using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

/// <summary>One destroy/repair choice emitted by an ALNS operator-selection session.</summary>
public readonly record struct AdaptiveLargeNeighborhoodOperatorSelection(
    int DestroyIndex,
    int RepairIndex,
    double DestroySelectionMetric,
    double RepairSelectionMetric);

/// <summary>
/// Stateless factory for one run-local ALNS operator-selection session.
/// </summary>
public interface IAdaptiveLargeNeighborhoodOperatorSelectionStrategy
{
    string Id { get; }

    IAdaptiveLargeNeighborhoodOperatorSelectionSession CreateSession(
        int destroyOperatorCount,
        int repairOperatorCount,
        AdaptiveLargeNeighborhoodSearchParameters parameters);
}

/// <summary>
/// Run-local mutable state of an ALNS operator-selection strategy.
/// </summary>
public interface IAdaptiveLargeNeighborhoodOperatorSelectionSession
{
    string StrategyId { get; }

    long SegmentUpdateCount { get; }

    AdaptiveLargeNeighborhoodOperatorSelection Select(
        IRandomSource random,
        int iteration);

    void RecordOutcome(
        in AdaptiveLargeNeighborhoodOperatorSelection selection,
        double reward);

    void CompleteIteration(
        int iteration);

    AdaptiveLargeNeighborhoodOperatorSelection GetCurrentSelectionMetrics(
        in AdaptiveLargeNeighborhoodOperatorSelection selection,
        int iteration);
}

/// <summary>
/// Canonical Ropke-Pisinger independently weighted segmented roulette selection.
/// </summary>
public sealed class IndependentSegmentedRouletteOperatorSelectionStrategy :
    IAdaptiveLargeNeighborhoodOperatorSelectionStrategy
{
    public static IndependentSegmentedRouletteOperatorSelectionStrategy Instance { get; } =
        new();

    private IndependentSegmentedRouletteOperatorSelectionStrategy()
    {
    }

    public string Id =>
        "alns.selection.roulette-independent";

    public IAdaptiveLargeNeighborhoodOperatorSelectionSession CreateSession(
        int destroyOperatorCount,
        int repairOperatorCount,
        AdaptiveLargeNeighborhoodSearchParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (destroyOperatorCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(destroyOperatorCount));

        if (repairOperatorCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(repairOperatorCount));

        return new Session(
            destroyOperatorCount,
            repairOperatorCount,
            parameters);
    }

    private sealed class Session :
        IAdaptiveLargeNeighborhoodOperatorSelectionSession
    {
        private readonly AdaptiveLargeNeighborhoodSearchParameters _parameters;
        private readonly double[] _destroyWeights;
        private readonly double[] _repairWeights;
        private readonly double[] _destroyScores;
        private readonly double[] _repairScores;
        private readonly int[] _destroyUsage;
        private readonly int[] _repairUsage;

        public Session(
            int destroyOperatorCount,
            int repairOperatorCount,
            AdaptiveLargeNeighborhoodSearchParameters parameters)
        {
            _parameters =
                parameters;

            _destroyWeights =
                CreateUniform(
                    destroyOperatorCount,
                    parameters.InitialOperatorWeight);

            _repairWeights =
                CreateUniform(
                    repairOperatorCount,
                    parameters.InitialOperatorWeight);

            _destroyScores =
                new double[destroyOperatorCount];

            _repairScores =
                new double[repairOperatorCount];

            _destroyUsage =
                new int[destroyOperatorCount];

            _repairUsage =
                new int[repairOperatorCount];
        }

        public string StrategyId =>
            IndependentSegmentedRouletteOperatorSelectionStrategy.Instance.Id;

        public long SegmentUpdateCount { get; private set; }

        public AdaptiveLargeNeighborhoodOperatorSelection Select(
            IRandomSource random,
            int iteration)
        {
            ArgumentNullException.ThrowIfNull(random);

            if (iteration <= 0)
                throw new ArgumentOutOfRangeException(nameof(iteration));

            int destroyIndex =
                AdaptiveLargeNeighborhoodAdaptation.SelectIndex(
                    _destroyWeights,
                    random);

            int repairIndex =
                AdaptiveLargeNeighborhoodAdaptation.SelectIndex(
                    _repairWeights,
                    random);

            return new AdaptiveLargeNeighborhoodOperatorSelection(
                destroyIndex,
                repairIndex,
                _destroyWeights[destroyIndex],
                _repairWeights[repairIndex]);
        }

        public void RecordOutcome(
            in AdaptiveLargeNeighborhoodOperatorSelection selection,
            double reward)
        {
            if (!double.IsFinite(reward) || reward < 0.0)
                throw new ArgumentOutOfRangeException(nameof(reward));

            _destroyScores[selection.DestroyIndex] +=
                reward;

            _repairScores[selection.RepairIndex] +=
                reward;

            _destroyUsage[selection.DestroyIndex]++;
            _repairUsage[selection.RepairIndex]++;
        }

        public void CompleteIteration(
            int iteration)
        {
            if (iteration <= 0)
                throw new ArgumentOutOfRangeException(nameof(iteration));

            if ((iteration % _parameters.SegmentLength) != 0)
                return;

            Update(
                _destroyWeights,
                _destroyScores,
                _destroyUsage,
                _parameters.ReactionFactor);

            Update(
                _repairWeights,
                _repairScores,
                _repairUsage,
                _parameters.ReactionFactor);

            SegmentUpdateCount++;
        }

        public AdaptiveLargeNeighborhoodOperatorSelection GetCurrentSelectionMetrics(
            in AdaptiveLargeNeighborhoodOperatorSelection selection,
            int iteration)
        {
            if (iteration <= 0)
                throw new ArgumentOutOfRangeException(nameof(iteration));

            return new AdaptiveLargeNeighborhoodOperatorSelection(
                selection.DestroyIndex,
                selection.RepairIndex,
                _destroyWeights[selection.DestroyIndex],
                _repairWeights[selection.RepairIndex]);
        }

        private static double[] CreateUniform(
            int length,
            double value)
        {
            var result =
                new double[length];

            Array.Fill(
                result,
                value);

            return result;
        }

        private static void Update(
            double[] weights,
            double[] scores,
            int[] usage,
            double reactionFactor)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] =
                    AdaptiveLargeNeighborhoodAdaptation.UpdateWeight(
                        weights[i],
                        scores[i],
                        usage[i],
                        reactionFactor);

                scores[i] =
                    0.0;

                usage[i] =
                    0;
            }
        }
    }
}
