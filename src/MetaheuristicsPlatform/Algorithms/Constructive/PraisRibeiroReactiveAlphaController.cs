using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// Per-run Reactive GRASP alpha controller following the Prais-Ribeiro probability update.
/// For minimization with strictly positive objective values,
/// q_i = z_best / A_i and p_i = q_i / sum_j q_j.
/// For maximization, the platform uses the sense-consistent mirrored ratio q_i = A_i / z_best.
/// </summary>
public sealed class PraisRibeiroReactiveAlphaController
{
    private readonly double[] _alphas;
    private readonly double[] _probabilities;
    private readonly double[] _means;
    private readonly long[] _counts;
    private readonly OptimizationSense _sense;
    private readonly int _probabilityUpdatePeriod;

    private long _observations;
    private int _distinctObserved;
    private int _probabilityUpdates;
    private bool _hasBest;
    private double _bestObjective;

    /// <summary>Creates one isolated controller for an optimization run.</summary>
    public PraisRibeiroReactiveAlphaController(
        IReadOnlyList<double> alphaValues,
        int probabilityUpdatePeriod,
        OptimizationSense sense)
    {
        ArgumentNullException.ThrowIfNull(alphaValues);

        if (alphaValues.Count == 0)
        {
            throw new ArgumentException(
                "Reactive GRASP requires at least one alpha value.",
                nameof(alphaValues));
        }

        if (probabilityUpdatePeriod <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(probabilityUpdatePeriod));
        }

        _alphas = new double[alphaValues.Count];
        _probabilities = new double[alphaValues.Count];
        _means = new double[alphaValues.Count];
        _counts = new long[alphaValues.Count];
        _sense = sense;
        _probabilityUpdatePeriod = probabilityUpdatePeriod;

        double uniform = 1.0 / alphaValues.Count;

        for (int i = 0; i < alphaValues.Count; i++)
        {
            double alpha = alphaValues[i];

            if (!double.IsFinite(alpha) || alpha < 0.0 || alpha > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(alphaValues),
                    "Every Reactive GRASP alpha value must be finite and lie in [0,1].");
            }

            for (int j = 0; j < i; j++)
            {
                if (_alphas[j].Equals(alpha))
                {
                    throw new ArgumentException(
                        "Reactive GRASP alpha values must be unique.",
                        nameof(alphaValues));
                }
            }

            _alphas[i] = alpha;
            _probabilities[i] = uniform;
        }

        _bestObjective = sense.WorstValue();
    }

    /// <summary>Number of configured discrete alpha values.</summary>
    public int Count => _alphas.Length;

    /// <summary>Number of distinct alpha values observed at least once.</summary>
    public int DistinctObserved => _distinctObserved;

    /// <summary>Number of successful probability-vector recomputations.</summary>
    public int ProbabilityUpdates => _probabilityUpdates;

    /// <summary>Returns an alpha value by stable controller index.</summary>
    public double GetAlpha(int index) => _alphas[index];

    /// <summary>Returns the current selection probability of an alpha index.</summary>
    public double GetProbability(int index) => _probabilities[index];

    /// <summary>Selects an alpha index from the current discrete probability distribution.</summary>
    public int SelectAlphaIndex(IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        double u = random.NextDouble();
        double cumulative = 0.0;

        for (int i = 0; i < _probabilities.Length - 1; i++)
        {
            cumulative += _probabilities[i];

            if (u < cumulative)
            {
                return i;
            }
        }

        return _probabilities.Length - 1;
    }

    /// <summary>
    /// Records the locally improved objective obtained with one alpha value.
    /// Probability recomputation is attempted periodically after every alpha has
    /// been observed at least once.
    /// </summary>
    public void Observe(
        int alphaIndex,
        double objective)
    {
        if ((uint)alphaIndex >= (uint)_alphas.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(alphaIndex));
        }

        if (!double.IsFinite(objective))
        {
            throw new ArgumentOutOfRangeException(
                nameof(objective),
                "Reactive GRASP objective observations must be finite.");
        }

        long count = ++_counts[alphaIndex];

        if (count == 1)
        {
            _distinctObserved++;
            _means[alphaIndex] = objective;
        }
        else
        {
            _means[alphaIndex] +=
                (objective - _means[alphaIndex]) / count;
        }

        if (!_hasBest ||
            _sense.IsBetter(
                objective,
                _bestObjective))
        {
            _bestObjective = objective;
            _hasBest = true;
        }

        _observations++;

        if ((_observations % _probabilityUpdatePeriod) == 0 &&
            _distinctObserved == _alphas.Length)
        {
            RecomputeProbabilities();
        }
    }

    private void RecomputeProbabilities()
    {
        if (!_hasBest)
        {
            return;
        }

        if (!(_bestObjective > 0.0))
        {
            throw new InvalidOperationException(
                "The canonical Prais-Ribeiro ratio update requires strictly positive objective values. " +
                "Use an objective transformation at the problem boundary when the natural objective can be zero or negative.");
        }

        double qualitySum = 0.0;

        for (int i = 0; i < _means.Length; i++)
        {
            double average = _means[i];

            if (!(average > 0.0))
            {
                throw new InvalidOperationException(
                    "The canonical Prais-Ribeiro ratio update requires strictly positive per-alpha average objective values.");
            }

            double quality =
                _sense == OptimizationSense.Minimize
                    ? _bestObjective / average
                    : average / _bestObjective;

            if (!double.IsFinite(quality) || quality <= 0.0)
            {
                throw new InvalidOperationException(
                    "Reactive GRASP produced an invalid Prais-Ribeiro quality weight.");
            }

            _probabilities[i] = quality;
            qualitySum += quality;
        }

        if (!double.IsFinite(qualitySum) || qualitySum <= 0.0)
        {
            throw new InvalidOperationException(
                "Reactive GRASP produced an invalid probability normalization constant.");
        }

        double inverse = 1.0 / qualitySum;

        for (int i = 0; i < _probabilities.Length; i++)
        {
            _probabilities[i] *= inverse;
        }

        _probabilityUpdates++;
    }
}
