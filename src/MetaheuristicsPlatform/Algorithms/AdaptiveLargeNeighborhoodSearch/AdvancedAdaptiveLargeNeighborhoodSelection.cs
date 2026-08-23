using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

/// <summary>
/// Learns a joint weight for each destroy/repair pair and samples pairs by segmented roulette.
/// </summary>
public sealed class PairCoupledSegmentedRouletteOperatorSelectionStrategy :
    IAdaptiveLargeNeighborhoodOperatorSelectionStrategy
{
    public string Id =>
        "alns.advanced.selection.pair-coupled-segmented-roulette";

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
        private readonly int _repairOperatorCount;
        private readonly AdaptiveLargeNeighborhoodSearchParameters _parameters;
        private readonly double[] _pairWeights;
        private readonly double[] _pairScores;
        private readonly int[] _pairUsage;

        public Session(
            int destroyOperatorCount,
            int repairOperatorCount,
            AdaptiveLargeNeighborhoodSearchParameters parameters)
        {
            _repairOperatorCount =
                repairOperatorCount;

            _parameters =
                parameters;

            int pairCount =
                checked(
                    destroyOperatorCount *
                    repairOperatorCount);

            _pairWeights =
                new double[pairCount];

            Array.Fill(
                _pairWeights,
                parameters.InitialOperatorWeight);

            _pairScores =
                new double[pairCount];

            _pairUsage =
                new int[pairCount];
        }

        public string StrategyId =>
            "alns.advanced.selection.pair-coupled-segmented-roulette";

        public long SegmentUpdateCount { get; private set; }

        public AdaptiveLargeNeighborhoodOperatorSelection Select(
            IRandomSource random,
            int iteration)
        {
            ArgumentNullException.ThrowIfNull(random);

            if (iteration <= 0)
                throw new ArgumentOutOfRangeException(nameof(iteration));

            int pairIndex =
                AdaptiveLargeNeighborhoodAdaptation.SelectIndex(
                    _pairWeights,
                    random);

            int destroyIndex =
                pairIndex /
                _repairOperatorCount;

            int repairIndex =
                pairIndex %
                _repairOperatorCount;

            double pairWeight =
                _pairWeights[pairIndex];

            return new AdaptiveLargeNeighborhoodOperatorSelection(
                destroyIndex,
                repairIndex,
                pairWeight,
                pairWeight);
        }

        public void RecordOutcome(
            in AdaptiveLargeNeighborhoodOperatorSelection selection,
            double reward)
        {
            if (!double.IsFinite(reward) || reward < 0.0)
                throw new ArgumentOutOfRangeException(nameof(reward));

            int pairIndex =
                checked(
                    (selection.DestroyIndex * _repairOperatorCount) +
                    selection.RepairIndex);

            _pairScores[pairIndex] +=
                reward;

            _pairUsage[pairIndex]++;
        }

        public void CompleteIteration(
            int iteration)
        {
            if (iteration <= 0)
                throw new ArgumentOutOfRangeException(nameof(iteration));

            if ((iteration % _parameters.SegmentLength) != 0)
                return;

            for (int i = 0; i < _pairWeights.Length; i++)
            {
                _pairWeights[i] =
                    AdaptiveLargeNeighborhoodAdaptation.UpdateWeight(
                        _pairWeights[i],
                        _pairScores[i],
                        _pairUsage[i],
                        _parameters.ReactionFactor);

                _pairScores[i] =
                    0.0;

                _pairUsage[i] =
                    0;
            }

            SegmentUpdateCount++;
        }

        public AdaptiveLargeNeighborhoodOperatorSelection GetCurrentSelectionMetrics(
            in AdaptiveLargeNeighborhoodOperatorSelection selection,
            int iteration)
        {
            if (iteration <= 0)
                throw new ArgumentOutOfRangeException(nameof(iteration));

            int pairIndex =
                checked(
                    (selection.DestroyIndex * _repairOperatorCount) +
                    selection.RepairIndex);

            double pairWeight =
                _pairWeights[pairIndex];

            return new AdaptiveLargeNeighborhoodOperatorSelection(
                selection.DestroyIndex,
                selection.RepairIndex,
                pairWeight,
                pairWeight);
        }
    }
}

/// <summary>
/// Hendel (2022) alpha-UCB operator-pair selection adapted to generic ALNS destroy/repair pairs.
/// </summary>
public sealed class AlphaUcbOperatorPairSelectionStrategy :
    IAdaptiveLargeNeighborhoodOperatorSelectionStrategy
{
    public AlphaUcbOperatorPairSelectionStrategy(
        double alpha = 0.05,
        double initialAverageReward = 1.0)
    {
        if (!double.IsFinite(alpha) ||
            alpha < 0.0 ||
            alpha > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(alpha));
        }

        if (!double.IsFinite(initialAverageReward) ||
            initialAverageReward < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialAverageReward));
        }

        Alpha =
            alpha;

        InitialAverageReward =
            initialAverageReward;
    }

    public string Id =>
        "alns.advanced.selection.alpha-ucb-hendel-2022";

    public double Alpha { get; }

    public double InitialAverageReward { get; }

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
            Alpha,
            InitialAverageReward);
    }

    private sealed class Session :
        IAdaptiveLargeNeighborhoodOperatorSelectionSession
    {
        private readonly int _repairOperatorCount;
        private readonly double _alpha;
        private readonly double[] _averageReward;
        private readonly int[] _plays;

        public Session(
            int destroyOperatorCount,
            int repairOperatorCount,
            double alpha,
            double initialAverageReward)
        {
            _repairOperatorCount =
                repairOperatorCount;

            _alpha =
                alpha;

            int pairCount =
                checked(
                    destroyOperatorCount *
                    repairOperatorCount);

            _averageReward =
                new double[pairCount];

            Array.Fill(
                _averageReward,
                initialAverageReward);

            _plays =
                new int[pairCount];
        }

        public string StrategyId =>
            "alns.advanced.selection.alpha-ucb-hendel-2022";

        public long SegmentUpdateCount =>
            0L;

        public AdaptiveLargeNeighborhoodOperatorSelection Select(
            IRandomSource random,
            int iteration)
        {
            ArgumentNullException.ThrowIfNull(random);

            if (iteration <= 0)
                throw new ArgumentOutOfRangeException(nameof(iteration));

            int pairIndex =
                SelectPair(
                    random,
                    iteration);

            int destroyIndex =
                pairIndex /
                _repairOperatorCount;

            int repairIndex =
                pairIndex %
                _repairOperatorCount;

            double score =
                ComputeScore(
                    pairIndex,
                    iteration);

            return new AdaptiveLargeNeighborhoodOperatorSelection(
                destroyIndex,
                repairIndex,
                score,
                score);
        }

        public void RecordOutcome(
            in AdaptiveLargeNeighborhoodOperatorSelection selection,
            double reward)
        {
            if (!double.IsFinite(reward) || reward < 0.0)
                throw new ArgumentOutOfRangeException(nameof(reward));

            int pairIndex =
                checked(
                    (selection.DestroyIndex * _repairOperatorCount) +
                    selection.RepairIndex);

            int previousPlays =
                _plays[pairIndex];

            _averageReward[pairIndex] =
                ((previousPlays * _averageReward[pairIndex]) + reward) /
                (previousPlays + 1);

            _plays[pairIndex] =
                previousPlays + 1;
        }

        public void CompleteIteration(
            int iteration)
        {
            if (iteration <= 0)
                throw new ArgumentOutOfRangeException(nameof(iteration));
        }

        public AdaptiveLargeNeighborhoodOperatorSelection GetCurrentSelectionMetrics(
            in AdaptiveLargeNeighborhoodOperatorSelection selection,
            int iteration)
        {
            if (iteration <= 0)
                throw new ArgumentOutOfRangeException(nameof(iteration));

            int pairIndex =
                checked(
                    (selection.DestroyIndex * _repairOperatorCount) +
                    selection.RepairIndex);

            double score =
                ComputeScore(
                    pairIndex,
                    iteration);

            return new AdaptiveLargeNeighborhoodOperatorSelection(
                selection.DestroyIndex,
                selection.RepairIndex,
                score,
                score);
        }

        private int SelectPair(
            IRandomSource random,
            int iteration)
        {
            var unseen =
                new List<int>();

            for (int i = 0; i < _plays.Length; i++)
            {
                if (_plays[i] == 0)
                    unseen.Add(i);
            }

            if (unseen.Count > 0)
            {
                return unseen[
                    random.NextInt32(
                        unseen.Count)];
            }

            int bestIndex =
                0;

            double bestScore =
                ComputeScore(
                    0,
                    iteration);

            for (int i = 1; i < _plays.Length; i++)
            {
                double score =
                    ComputeScore(
                        i,
                        iteration);

                if (score > bestScore)
                {
                    bestScore =
                        score;

                    bestIndex =
                        i;
                }
            }

            return bestIndex;
        }

        private double ComputeScore(
            int pairIndex,
            int iteration)
        {
            if (_plays[pairIndex] == 0)
                return double.PositiveInfinity;

            double exploration =
                Math.Sqrt(
                    (_alpha * Math.Log(1.0 + iteration)) /
                    _plays[pairIndex]);

            return
                _averageReward[pairIndex] +
                exploration;
        }
    }
}
