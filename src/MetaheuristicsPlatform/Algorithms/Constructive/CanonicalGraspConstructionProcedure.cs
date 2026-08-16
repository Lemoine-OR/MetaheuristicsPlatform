using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// Canonical threshold-RCL GRASP construction with allocation-free two-pass candidate scans.
/// The first pass computes the score range; the second performs uniform reservoir sampling
/// over the restricted candidate list without materializing that list.
/// </summary>
public sealed class CanonicalGraspConstructionProcedure<
    TSolution,
    TCandidate,
    TEnumerator> : IGraspConstructionProcedure<TSolution>
    where TEnumerator : struct, IGraspCandidateEnumerator<TCandidate>
{
    private readonly IGraspConstructionModel<TSolution, TCandidate, TEnumerator> _model;

    /// <summary>Creates the canonical construction engine.</summary>
    public CanonicalGraspConstructionProcedure(
        IGraspConstructionModel<TSolution, TCandidate, TEnumerator> model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));

        if (!Enum.IsDefined(_model.GreedyScoreSense))
        {
            throw new ArgumentOutOfRangeException(
                nameof(model),
                "The construction model exposes an invalid greedy-score sense.");
        }
    }

    /// <inheritdoc />
    public GraspConstructionResult<TSolution> Construct(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random,
        double alpha,
        int maximumConstructionSteps,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(random);

        if (!double.IsFinite(alpha) || alpha < 0.0 || alpha > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(alpha));
        }

        if (maximumConstructionSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumConstructionSteps));
        }

        TSolution solution =
            _model.CreateInitialSolution(problem, random);

        int constructionSteps = 0;
        long scoreEvaluations = 0;

        while (!_model.IsComplete(in solution, problem))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (constructionSteps >= maximumConstructionSteps)
            {
                throw new InvalidOperationException(
                    "GRASP construction exceeded MaximumConstructionSteps before reaching a complete solution.");
            }

            TEnumerator rangeEnumerator =
                _model.GetCandidateEnumerator(in solution, problem);

            bool found = false;
            double best = 0.0;
            double worst = 0.0;

            while (rangeEnumerator.MoveNext(out TCandidate candidate))
            {
                cancellationToken.ThrowIfCancellationRequested();

                double score =
                    _model.EvaluateGreedyScore(
                        in solution,
                        in candidate,
                        problem);

                scoreEvaluations++;

                if (!double.IsFinite(score))
                {
                    throw new InvalidOperationException(
                        "GRASP greedy scores must be finite.");
                }

                if (!found)
                {
                    found = true;
                    best = score;
                    worst = score;
                    continue;
                }

                if (IsBetter(score, best, _model.GreedyScoreSense))
                {
                    best = score;
                }

                if (IsBetter(worst, score, _model.GreedyScoreSense))
                {
                    worst = score;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    "GRASP construction is incomplete but the candidate list is empty.");
            }

            double threshold =
                ComputeThreshold(
                    best,
                    worst,
                    alpha,
                    _model.GreedyScoreSense);

            TEnumerator selectionEnumerator =
                _model.GetCandidateEnumerator(in solution, problem);

            bool selected = false;
            TCandidate selectedCandidate = default!;
            int restrictedCandidateCount = 0;

            while (selectionEnumerator.MoveNext(out TCandidate candidate))
            {
                cancellationToken.ThrowIfCancellationRequested();

                double score =
                    _model.EvaluateGreedyScore(
                        in solution,
                        in candidate,
                        problem);

                scoreEvaluations++;

                if (!double.IsFinite(score))
                {
                    throw new InvalidOperationException(
                        "GRASP greedy scores must be finite.");
                }

                if (!Qualifies(
                    score,
                    threshold,
                    _model.GreedyScoreSense))
                {
                    continue;
                }

                restrictedCandidateCount++;

                if (random.NextInt32(restrictedCandidateCount) == 0)
                {
                    selected = true;
                    selectedCandidate = candidate;
                }
            }

            if (!selected)
            {
                throw new InvalidOperationException(
                    "GRASP restricted candidate list is unexpectedly empty.");
            }

            _model.ApplyCandidate(
                ref solution,
                in selectedCandidate,
                problem);

            constructionSteps++;
        }

        return new GraspConstructionResult<TSolution>(
            solution,
            constructionSteps,
            scoreEvaluations);
    }

    private static bool IsBetter(
        double left,
        double right,
        GraspGreedyScoreSense sense) =>
        sense switch
        {
            GraspGreedyScoreSense.Minimize => left < right,
            GraspGreedyScoreSense.Maximize => left > right,
            _ => throw new ArgumentOutOfRangeException(nameof(sense))
        };

    private static double ComputeThreshold(
        double best,
        double worst,
        double alpha,
        GraspGreedyScoreSense sense) =>
        sense switch
        {
            GraspGreedyScoreSense.Minimize =>
                best + alpha * (worst - best),
            GraspGreedyScoreSense.Maximize =>
                best - alpha * (best - worst),
            _ => throw new ArgumentOutOfRangeException(nameof(sense))
        };

    private static bool Qualifies(
        double score,
        double threshold,
        GraspGreedyScoreSense sense) =>
        sense switch
        {
            GraspGreedyScoreSense.Minimize => score <= threshold,
            GraspGreedyScoreSense.Maximize => score >= threshold,
            _ => throw new ArgumentOutOfRangeException(nameof(sense))
        };
}
