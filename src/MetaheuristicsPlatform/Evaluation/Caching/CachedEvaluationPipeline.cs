using MetaheuristicsPlatform.Evaluation.Instrumentation;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Evaluation.Caching;

/// <summary>
/// Complete-outcome caching decorator for an evaluation pipeline.
/// </summary>
public sealed class CachedEvaluationPipeline<TCandidate, TSolution, TKey> :
    IEvaluationPipeline<TCandidate, TSolution>
    where TKey : notnull
{
    private readonly IEvaluationPipeline<TCandidate, TSolution> _inner;
    private readonly IEvaluationCacheKeySelector<TCandidate, TKey> _keySelector;
    private readonly IEvaluationCache<
        TKey,
        EvaluationCacheEntry<TCandidate, TSolution>> _cache;
    private readonly IEvaluationSnapshotCloner<TSolution> _solutionCloner;
    private readonly IEvaluationSnapshotCloner<TCandidate>? _candidateCloner;
    private readonly IEvaluationPipelineMetricsSink? _metricsSink;

    public CachedEvaluationPipeline(
        IEvaluationPipeline<TCandidate, TSolution> inner,
        IEvaluationCacheKeySelector<TCandidate, TKey> keySelector,
        IEvaluationCache<
            TKey,
            EvaluationCacheEntry<TCandidate, TSolution>> cache,
        IEvaluationSnapshotCloner<TSolution> solutionCloner,
        IEvaluationSnapshotCloner<TCandidate>? candidateCloner = null,
        IEvaluationPipelineMetricsSink? metricsSink = null)
    {
        _inner =
            inner ??
            throw new ArgumentNullException(
                nameof(inner));

        _keySelector =
            keySelector ??
            throw new ArgumentNullException(
                nameof(keySelector));

        _cache =
            cache ??
            throw new ArgumentNullException(
                nameof(cache));

        _solutionCloner =
            solutionCloner ??
            throw new ArgumentNullException(
                nameof(solutionCloner));

        _candidateCloner = candidateCloner;
        _metricsSink = metricsSink;

        if (inner.FeedbackMode ==
                ImprovementFeedbackMode.Lamarckian &&
            candidateCloner is null)
        {
            throw new ArgumentException(
                "Lamarckian caching requires an independent candidate snapshot cloner.",
                nameof(candidateCloner));
        }
    }

    public ImprovementFeedbackMode FeedbackMode =>
        _inner.FeedbackMode;

    public EvaluationCharacteristics
        EvaluationCharacteristics =>
        _inner.EvaluationCharacteristics;

    public int CachedOutcomeCount =>
        _cache.Count;

    public EvaluationPipelineResult<TSolution> Evaluate(
        ref TCandidate candidate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TKey key =
            _keySelector.GetKey(
                candidate);

        TCandidate inputCandidate =
            candidate;

        EvaluationCacheLookup<
            EvaluationCacheEntry<TCandidate, TSolution>> lookup =
            _cache.GetOrAdd(
                key,
                _ =>
                    EvaluateAndSnapshot(
                        inputCandidate,
                        cancellationToken));

        if (lookup.IsHit)
        {
            _metricsSink?.RecordCacheHit();
        }
        else
        {
            _metricsSink?.RecordCacheMiss();
        }

        EvaluationCacheEntry<TCandidate, TSolution> entry =
            lookup.Value;

        if (entry.HasCandidateSnapshot)
        {
            candidate =
                _candidateCloner!.Clone(
                    entry.CandidateSnapshot);
        }

        TSolution returnedSolution =
            _solutionCloner.Clone(
                entry.SolutionSnapshot);

        return new EvaluationPipelineResult<TSolution>(
            entry.Fitness,
            returnedSolution,
            entry.WasRepaired,
            entry.WasImproved,
            entry.FeedbackApplied);
    }

    public void ClearCache() =>
        _cache.Clear();

    private EvaluationCacheEntry<TCandidate, TSolution>
        EvaluateAndSnapshot(
            TCandidate inputCandidate,
            CancellationToken cancellationToken)
    {
        TCandidate workingCandidate =
            FeedbackMode ==
                ImprovementFeedbackMode.Lamarckian
                ? _candidateCloner!.Clone(
                    inputCandidate)
                : inputCandidate;

        EvaluationPipelineResult<TSolution> result =
            _inner.Evaluate(
                ref workingCandidate,
                cancellationToken);

        TSolution solutionSnapshot =
            _solutionCloner.Clone(
                result.Solution);

        bool hasCandidateSnapshot =
            FeedbackMode ==
            ImprovementFeedbackMode.Lamarckian;

        TCandidate candidateSnapshot =
            hasCandidateSnapshot
                ? _candidateCloner!.Clone(
                    workingCandidate)
                : default!;

        return new EvaluationCacheEntry<TCandidate, TSolution>(
            result.Fitness,
            solutionSnapshot,
            candidateSnapshot,
            hasCandidateSnapshot,
            result.WasRepaired,
            result.WasImproved,
            result.FeedbackApplied);
    }
}